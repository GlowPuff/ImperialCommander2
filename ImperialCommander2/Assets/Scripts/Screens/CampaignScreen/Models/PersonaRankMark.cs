namespace Saga
{
	// Personas (M2, Thomas 02.11.): announcement bookkeeping for the campaign
	// persona. The LIVE rank is ALWAYS recomputed as a pure function of
	// (persona, live mission threat) - Core.Personas rank law; this row only
	// remembers which rank was LAST announced at the table, so a rank change
	// (up OR down) can be named exactly once with its direction and old
	// value. ADDITIVE campaign field (personaRank on SagaCampaign), may be
	// null in saves written before the persona era (= nothing announced).
	public class PersonaRankMark
	{
		public string personaId;   // the persona this mark belongs to (asset id)
		public int rank;           // the rank that was announced
	}
}
