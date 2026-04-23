using System;
using UnityEngine;

public class QuestionNode : ITreeeNode
{
    private ITreeeNode trueNode;
    private ITreeeNode flaseNode;
    private Func<bool> question;

    public QuestionNode(Func<bool> question, ITreeeNode trueNode, ITreeeNode falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.flaseNode = falseNode;
    }
    public void Execute()
    {
        if(question.Invoke())
            trueNode.Execute();
        else
            flaseNode.Execute();    
    }
}
