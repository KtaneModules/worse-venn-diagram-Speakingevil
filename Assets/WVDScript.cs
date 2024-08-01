using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WVDScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> segments;
    public Renderer[] segrends;
    public Material[] segmats;
    public TextMesh display;

    private readonly Color[] dcols = new Color[3] { new Color(1, 0, 0), new Color(0, 0, 1), new Color(0, 1, 0)};
    private readonly string[] olog = new string[14] { "\u2227", "\u2228", "\u22bb", "\u21a6", "\u22a5", "\u00d7", "\u25b3", "\u21e5", "&", "\u002b", "\u2295", "\u21e8", "\u207a", "\u207b" };
    private readonly int[,] op = new int[12, 9] { { 0, 0, 0, 0, 1, 1, 0, 1, 2}, { 0, 1, 2, 1, 1, 2, 2, 2, 2}, { 0, 1, 2, 1, 1, 1, 2, 1, 0}, { 2, 2, 2, 1, 1, 2, 0, 1, 2}, { 0, 1, 0, 1, 2, 1, 0, 1, 0}, { 1, 2, 0, 2, 0, 2, 0, 2, 1}, { 0, 2, 1, 2, 1, 0, 1, 0, 2}, { 2, 2, 2, 0, 1, 2, 0, 0, 2}, { 0, 0, 1, 0, 0, 1, 1, 1, 2}, { 0, 0, 2, 0, 1, 2, 2, 2, 2}, { 0, 1, 1, 1, 2, 2, 1, 2, 2}, { 1, 1, 2, 0, 0, 2, 0, 0, 2} };
    private int[] v = new int[3];
    private int[] bins = new int[2];
    private int[] uns = new int[4];
    private int bracket;
    private int[,] outputs = new int[5, 27];
    private bool[] select = new bool[27];
    private int target;
    private string[] snippets = new string[4];

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        v = Enumerable.Range(0, 3).ToArray().Shuffle().ToArray();
        for (int i = 0; i < 3; i++)
            snippets[i] = "ABC"[v[i]].ToString();
        for (int i = 0; i < 2; i++)
            bins[i] = Random.Range(0, 12);
        List<int> l = new List<int> { 0, 0, 1, 2, 3, 4, 5}.Shuffle();
        int[] o = Enumerable.Range(0, 4).ToArray().Shuffle().ToArray();
        for(int i = 0; i < Mathf.Max(2, 4 - l[0]); i++)
        {
            l = l.Shuffle();
            uns[o[i]] = l[0];
        }
        bracket = Random.Range(0, 2);
        int u = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 27; j++)
                switch (v[i])
                {
                    case 0: outputs[i, j] = j / 9; break;
                    case 1: outputs[i, j] = (j / 3) % 3; break;
                    default: outputs[i, j] = j % 3; break;
                }
            snippets[i] = "ABC"[v[i]].ToString();
            u = uns[v[i]];
            if (u > 3)
            {
                snippets[i] += olog[13];
                for (int j = 0; j < 27; j++)
                {
                    outputs[i, j] += 2;
                    outputs[i, j] %= 3;
                }
            }
            else if (u > 1)
            {
                snippets[i] += olog[12];
                for (int j = 0; j < 27; j++)
                {
                    outputs[i, j]++;
                    outputs[i, j] %= 3;
                }
            }
            if (u % 2 == 1)
            {
                snippets[i] = "!" + snippets[i];
                for (int j = 0; j < 27; j++)
                    outputs[i, j] = 2 - outputs[i, j];
            }
        }
        snippets[3] = "(" + snippets[bracket] + olog[bins[bracket]] + snippets[bracket + 1] + ")";
        u = uns[3];
        if (u > 3)
        {
            snippets[3] += olog[13];
            for (int j = 0; j < 27; j++)
            {
                outputs[3, j] += 2;
                outputs[3, j] %= 3;
            }
        }
        else if (u > 1)
        {
            snippets[3] += olog[12];
            for (int j = 0; j < 27; j++)
            {
                outputs[3, j]++;
                outputs[3, j] %= 3;
            }
        }
        if (u % 2 == 1)
        {
            snippets[3] = "!" + snippets[3];
            for (int j = 0; j < 27; j++)
                outputs[3, j] = 2 - outputs[3, j];
        }
        for (int i = 0; i < 27; i++)
            outputs[3, i] = op[bins[bracket], (outputs[bracket, i] * 3) + outputs[bracket + 1, i]];
        if (bracket > 0)
        {
            display.text = snippets[0] + olog[bins[0]] + snippets[3];
            for (int i = 0; i < 27; i++)
                outputs[4, i] = op[bins[0], (outputs[0, i] * 3) + outputs[3, i]];
        }
        else
        {
            display.text = snippets[3] + olog[bins[1]] + snippets[2];
            for (int i = 0; i < 27; i++)
                outputs[4, i] = op[bins[1], (outputs[3, i] * 3) + outputs[2, i]];
        }
        target = Random.Range(0, 3);
        while(Enumerable.Range(0, 27).All(x => outputs[4, x] != target))
        {
            target++;
            target %= 3;
        }
        display.color = dcols[target];
        Debug.LogFormat("[Worse Venn Diagram #{0}] The displayed expression is \"{1}\"", moduleID, display.text);
        Debug.LogFormat("[Worse Venn Diagram #{0}] Select the areas the return {1}: {2}{3}", moduleID, new string[] { "False", "Unknown", "True"}[target], outputs[4, 0] == target ? "O, " : "", string.Join(", ", Enumerable.Range(1, 26).Where(x => outputs[4, x] == target).Select(x => "#aA"[x / 9].ToString() + "#bB"[(x / 3) % 3].ToString() + "#cC"[x % 3].ToString()).ToArray()).Replace("#", ""));
        foreach(KMSelectable seg in segments)
        {
            int s = segments.IndexOf(seg);
            seg.OnInteract += delegate ()
            {
                if (!moduleSolved && !select[s])
                {
                    select[s] = true;
                    seg.AddInteractionPunch(0.2f);
                    segrends[s].material = segmats[outputs[4, s]];
                    Audio.PlaySoundAtTransform("tick", seg.transform);
                    if (outputs[4, s] != target)
                    {
                        module.HandleStrike();
                        Debug.LogFormat("[Worse Venn Diagram #{0}] Incorrect area selected: {1}{2}", moduleID, s == 0 ? "O" : "", ("#aA"[s / 9].ToString() + "#bB"[(s / 3) % 3].ToString() + "#cC"[s % 3].ToString()).Replace("#", ""));
                    }
                    else if (Enumerable.Range(0, 27).All(x => outputs[4, x] != target || select[x]))
                    {
                        moduleSolved = true;
                        Debug.LogFormat("[Worse Venn Diagram #{0}] All correct areas selected.", moduleID);
                        module.HandlePass();
                        display.text = "";
                        Audio.PlaySoundAtTransform("Solve", transform);
                        StartCoroutine("SolveAnim");
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator SolveAnim()
    {
        int c = select.Count(x => !x);
        int[] r = Enumerable.Range(0, 27).ToArray().Shuffle().ToArray();
        for (int i = 0; i < 27; i++)
        {
            int p = r[i];
            if (!select[p])
            {
                segrends[p].material = segmats[outputs[4, p]];
                yield return new WaitForSeconds(0.3f / c);
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <O/<aA><bB><cC>> [Selects segments: where O is all False, lowercase is Unknown, and uppercase is True. Chain with spaces.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.Split(' ');
        List<int> s = new List<int> { };
        for(int i = 0; i < commands.Length; i++)
        {
            if (commands[i].Length < 1)
                continue;
            if (commands[i] == "O")
                s.Add(0);
            else if(commands[i].All(x => "aAbBcC".Contains(x.ToString())) && commands[i].Count(x => "aA".Contains(x.ToString())) < 2 && commands[i].Count(x => "bB".Contains(x.ToString())) < 2 && commands[i].Count(x => "cC".Contains(x.ToString())) < 2)
            {
                int d = commands[i].Any(x => x == 'a') ? 1 : commands[i].Any(x => x == 'A') ? 2 : 0;
                d *= 3;
                d += commands[i].Any(x => x == 'b') ? 1 : commands[i].Any(x => x == 'B') ? 2 : 0;
                d *= 3;
                d += commands[i].Any(x => x == 'c') ? 1 : commands[i].Any(x => x == 'C') ? 2 : 0;
                if (!select[d])
                    s.Add(d);
            }
            else
            {
                yield return "sendtochaterror!f " + commands[i] + " is an invalid segment.";
                yield break;
            }
        }
        for(int i = 0; i < s.Count(); i++)
        {
            yield return null;
            segments[s[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int[] r = Enumerable.Range(0, 27).ToArray().Shuffle().ToArray();
        for(int i = 0; i < 27; i++)
        {
            int p = r[i];
            if(!select[p] && outputs[4, p] == target)
            {
                yield return null;
                segments[p].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
