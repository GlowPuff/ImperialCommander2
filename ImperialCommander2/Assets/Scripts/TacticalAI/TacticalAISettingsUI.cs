// M1 settings UI for the settings dialog (concept 4.2 "Settings").
//
// Dev 23 REBUILD (Thomas: "Layout ist eine Katastrophe... alles muss da sein,
// korrekt, klickbar"). The template-clone approach (dev 18-22) is GONE - it
// depended on prefab internals that differ between the two settings dialogs
// and kept failing in the field. The root causes cloning could never fix:
//  * a Button on a GameObject WITHOUT any Graphic receives no raycast ->
//    cloned rows looked right but were unclickable ("klickbar ist nichts").
//  * host layout groups size children differently per dialog
//    (childControlHeight true/false) -> our LayoutElement heights were
//    ignored or rows exploded; rows vanished under the scroll clip.
// v3 = functional determinism, zero prefab dependencies:
//  * every row is a self-contained widget: dark Image (raycast source AND
//    hover background) + Button + TMP text. Nothing is cloned, so nothing
//    can silently be "missing".
//  * total height is pushed into LayoutElement AND sizeDelta AND a
//    ContentSizeFitter, so it renders fully in scrolling hosts, VLG hosts
//    and unmanaged hosts alike.
//  * yellow-bold TMP approximates the dialog look; per Thomas (27.10) the
//    cosmetic polish ranks BELOW function now.
//
// Behaviour (unchanged): master gates the sub-menu; click cycles values;
// 'legacy' is not offered globally (dev 15); the ladder is the single
// difficulty dial (dev 20); values render localized + caps (dev 22).
// M13g (Thomas 28.10): the old deployment-mode row is GONE - the app is a
// companion to the board game and the printed rules never hide a
// deployment: Aufstellungen sind IMMER SICHTBAR (DP highlight + camera
// move + TextBox, exactly like the legacy original).
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TacticalAI
{
    public static class TacticalAISettingsUI
    {
        const float TitleH = 40f;
        const float RowH = 36f;
        const float Gap = 6f;
        static readonly Color Yellow = new Color(0.93f, 0.79f, 0.17f);
        static readonly Color RowBg = new Color(0f, 0f, 0f, 0.45f);

        class Row
        {
            public GameObject go;
            public TMPro.TextMeshProUGUI tmp;
            public Button button;
            public Image bg;
            public void Set(string s) { if (tmp != null) tmp.text = s; }
        }

        static Transform _root;
        static readonly List<string> _subKinds = new List<string>();
        static readonly Dictionary<string, Row> _rows = new Dictionary<string, Row>();

        public static bool TryInstall(GameObject panel)
        {
            if (panel == null) return false;
            if (TMPro.TMP_Settings.defaultFontAsset == null)
            {
                Debug.Log("TacticalAISettingsUI::no TMP default font asset, skipping install");
                return false;
            }

            // Always rebuild fresh (dev 23): a stale root from an earlier
            // version living inside a re-used panel must never survive.
            var stale = FindDeep(panel.transform, "tacaiRoot");
            if (stale != null) UnityEngine.Object.Destroy(stale.gameObject);
            _rows.Clear();
            _subKinds.Clear();

            var sr = panel.GetComponentInChildren<ScrollRect>(true);
            var host = (sr != null && sr.content != null) ? sr.content : panel.transform;
            Debug.Log("TacticalAISettingsUI::install v3 panel='" + panel.name
                      + "' host='" + host.name + "' scroll=" + (sr != null)
                      + " chain='" + Chain(host) + "'");

            var go = new GameObject("tacaiRoot", typeof(RectTransform));
            go.transform.SetParent(host, false);
            _root = go.transform;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;    // v3: BOTH controls on, so our
            vlg.childControlHeight = true;   // row LayoutElement heights rule
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = Gap;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            go.AddComponent<LayoutElement>();
            // belt AND braces: hosts whose VLG has childControlHeight=false
            // ignore LayoutElement - they size by sizeDelta, so set it here
            // too (dev 23: managed hosts must not clip to the 100px default).
            var initrt = go.GetComponent<RectTransform>();
            float iw = 420f;
            var irt = host as RectTransform;
            if (irt != null && irt.rect.width > 80f) iw = irt.rect.width - 20f;
            initrt.sizeDelta = new Vector2(iw, 200f);
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Unmanaged host (no layout group): pin under the lowest sibling.
            if (host.GetComponent<LayoutGroup>() == null)
            {
                float width = 420f, lowest = 0f;
                var hrt = host as RectTransform;
                if (hrt != null && hrt.rect.width > 80f) width = hrt.rect.width - 20f;
                foreach (Transform sib in host)
                    if (sib != go.transform && sib.localPosition.y < lowest)
                        lowest = sib.localPosition.y;
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(width / 2f + 10f, lowest - 10f);
                rt.sizeDelta = new Vector2(width, 200f); // y recomputed in Refresh
            }

            MakeTitle();
            MakeRow(go.transform, "master", () =>
            {
                TacticalAISettings.Enabled = !TacticalAISettings.Enabled;
                Refresh();
            }, true);
            MakeRow(go.transform, "skill", () =>
            {
                TacticalAISettings.SkillLevel = Next(TacticalAISettings.SkillLevel, TacticalAISettings.SkillValues);
                Refresh();
            });
            MakeRow(go.transform, "explain", () =>
            {
                TacticalAISettings.ExplainLevel = Next(TacticalAISettings.ExplainLevel, TacticalAISettings.ExplainLevelValues);
                Refresh();
            });
            // M13c (Thomas 30.10): show/hide the general RR info block under
            // the action texts - same row style as EXPLAIN, default ON.
            // Personas (M1): global default persona (the campaign picker
            // follows it while its own field is empty). Same row style.
            MakeRow(go.transform, "persona", () =>
            {
                PersonaState.CycleDefault();
                Refresh();
            });
            MakeRow(go.transform, "info", () =>
            {
                TacticalAISettings.InfoText = !TacticalAISettings.InfoText;
                Refresh();
            });
            // M13h: the old non-clickable note line under the rows ("From S4 on
            // the AI plays the printed card abilities ...") is REMOVED by Thomas'
            // decision 28.10: it referenced internal 'S4' labels no player ever
            // sees (difficulty rows show renamed names) and added nothing that a
            // row does not already say. Do not reintroduce a note line here.

            Refresh();
            return true;
        }

        static string Chain(Transform t)
        {
            var sb = new System.Text.StringBuilder();
            for (var c = t; c != null; c = c.parent)
            {
                sb.Insert(0, c.name);
                if (c.parent != null) sb.Insert(0, "/");
                if (sb.Length > 160) break;
            }
            return sb.ToString();
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        // ---------------------------------------------------------- widgets

        static void MakeTitle()
        {
            var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
            var go = new GameObject("tacai_title", typeof(RectTransform));
            go.transform.SetParent(_root, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = false;
            MakeText(go.transform, tl != null ? tl.setHeading : "TACTICAL AI",
                TMPro.TextAlignmentOptions.Center, true);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = TitleH; le.minHeight = TitleH;
        }

        static void MakeRow(Transform parent, string kind, Action onClick, bool master = false)
        {
            var go = new GameObject("tacai_row_" + kind, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // THE click fix: a Button only receives raycasts through a
            // Graphic on its own GameObject (dev 18-22 bug: empty GO rows
            // looked right, ate nothing).
            var img = go.AddComponent<Image>();
            img.color = RowBg;
            img.raycastTarget = true;

            var b = go.AddComponent<Button>();
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor = RowBg;
            cb.highlightedColor = new Color(0f, 0.5f, 0.42f, 0.85f);
            cb.pressedColor = new Color(0f, 0.62f, 0.52f, 0.95f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(0f, 0f, 0f, 0.25f);
            b.colors = cb;
            b.targetGraphic = img;

            var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
            var t = MakeText(go.transform, RowText(kind, tl, TacticalAISettings.Enabled),
                TMPro.TextAlignmentOptions.MidlineLeft, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = RowH; le.minHeight = RowH;

            b.onClick.AddListener(() =>
            {
                var snd = GameObject.FindObjectOfType<Sound>();
                if (snd != null) snd.PlaySound(FX.Click);
                onClick();
            });

            _rows[kind] = new Row { go = go, tmp = t, button = b, bg = img };
            if (!master) _subKinds.Add(kind);
        }

        static TMPro.TextMeshProUGUI MakeText(Transform parent, string text,
            TMPro.TextAlignmentOptions align, bool title)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TMPro.TextMeshProUGUI>();
            t.font = TMPro.TMP_Settings.defaultFontAsset;
            t.text = text;
            t.color = Yellow;
            t.alignment = align;
            t.fontStyle = title ? (TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase)
                                : TMPro.FontStyles.Bold;
            t.enableAutoSizing = true;
            t.fontSizeMax = title ? 22f : 19f;
            t.fontSizeMin = 10f;
            t.raycastTarget = false;
            t.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(title ? 4f : 14f, 2f);
            rt.offsetMax = new Vector2(-4f, -2f);
            return t;
        }

        // ----------------------------------------------------------- refresh

        static void Refresh()
        {
            if (_root == null) return;
            var tl = DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null;
            bool on = TacticalAISettings.Enabled;

            foreach (var kv in _rows)
            {
                kv.Value.Set(RowText(kv.Key, tl, on));
                bool sub = _subKinds.Contains(kv.Key);
                kv.Value.go.SetActive(!sub || on);   // sub-menu hidden while OFF
            }

            // M13h: note line removed (see Build() comment) - height = title +
            // master row + visible sub rows only.
            float h = TitleH + RowH + Gap
                      + (on ? _subKinds.Count * (RowH + Gap) : 0f)
                      + 12f;
            var le = _root.GetComponent<LayoutElement>();
            if (le != null) { le.preferredHeight = h; le.minHeight = h; }
            var nrt = _root.GetComponent<RectTransform>();
            if (nrt != null)
            {
                var sd = nrt.sizeDelta;
                nrt.sizeDelta = new Vector2(sd.x, h);
            }
        }

        // ------------------------------------------------------------- text

        static string RowText(string kind, UITacticalAI tl, bool on)
        {
            string value, heading;
            switch (kind)
            {
                case "master": value = on ? (tl != null ? tl.setOn : "ON") : (tl != null ? tl.setOff : "OFF"); break;
                case "mode": value = TacticalAISettings.Mode.ToUpperInvariant(); break;
                case "skill": value = SkillDisplay(TacticalAISettings.SkillLevel, tl); break;
                case "explain":
                    value = TacticalAISettings.ExplainLevel == "none"
                        ? Pick(tl != null ? tl.expNone : null, "NONE")
                        : TacticalAISettings.ExplainLevel == "full"
                            ? Pick(tl != null ? tl.expFull : null, "FULL")
                            : Pick(tl != null ? tl.expPostHoc : null, "POST-HOC");
                    break;
                case "persona":
                    value = PersonaState.DescribeDefault();
                    break;
                case "info":
                    value = TacticalAISettings.InfoText
                        ? Pick(tl != null ? tl.setOn : null, "ON")
                        : Pick(tl != null ? tl.setOff : null, "OFF");
                    break;
                default: return kind;
            }
            bool master = kind == "master";
            if (tl == null) heading = master ? "Tactical AI" : kind.ToUpperInvariant();
            else if (master) heading = tl.setHeading;
            else if (kind == "mode") heading = tl.setMode;
            else if (kind == "skill") heading = tl.setSkill;
            else if (kind == "explain") heading = tl.setExplain;
            else if (kind == "persona") heading = tl.setPersona;   // personas row
            else heading = tl.setInfo;   // M13c info-block row
            // dialog rows render in caps like the Rundenlimit neighbours
            return (tl != null ? tl.setRowFormat : "{0}:  {1}")
                   .Replace("{0}", heading).Replace("{1}", value.ToUpperInvariant());
        }

        static string Pick(string localized, string fallback)
        {
            return string.IsNullOrEmpty(localized) ? fallback : localized;
        }

        /// <summary>DISPLAY via language files (themed ladder Rekrut..Grand
        /// Admiral Thrawn); stored/logged token stays s1..s5.</summary>
        static string SkillDisplay(string token, UITacticalAI tl)
        {
            if (string.IsNullOrEmpty(token)) return "";
            // dev 24 fix: stored tokens are UPPERCASE "S1".."S5" (Core
            // SkillValues) - the original lowercase cases never matched and
            // the row showed the raw token instead of the themed name.
            switch (token.ToUpperInvariant())
            {
                case "S1": return Pick(tl != null ? tl.skillS1 : null, "S1");
                case "S2": return Pick(tl != null ? tl.skillS2 : null, "S2");
                case "S3": return Pick(tl != null ? tl.skillS3 : null, "S3");
                case "S4": return Pick(tl != null ? tl.skillS4 : null, "S4");
                case "S5": return Pick(tl != null ? tl.skillS5 : null, "S5");
                default: return token.ToUpperInvariant();
            }
        }

        static string Next(string current, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] == current) return values[(i + 1) % values.Length];
            return values[0];
        }
    }
}
