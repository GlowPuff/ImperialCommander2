using System.Collections.Generic;

public class CardInstruction
{
	public string instName, instID;
	public List<InstructionOption> content;
}

public class InstructionOption
{
	public List<string> instruction;
}