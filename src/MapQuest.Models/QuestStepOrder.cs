namespace MapQuest.Models;

public class QuestStepOrder: DocumentModel
{
    public string StepId { get; set; } = null!;
    public int StepIndex { get; set; }

}
