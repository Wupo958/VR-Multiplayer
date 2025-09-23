using UnityEngine;
using TMPro;
using System.Linq;
using System.Text;
using System.Globalization;

public class updateLeaderboard : MonoBehaviour
{
    [Tooltip("Index 0 = Spieler 1, Index 1 = Spieler 2, ...")]
    public float[] playerDist = {1,7,2,0};

    [Tooltip("Ziel-Textfeld (TextMeshPro oder TextMeshProUGUI)")]
    public TMP_Text text;

    [Header("Anzeige-Optionen")]
    [SerializeField] private bool groesserIstBesser = true;   // true: absteigend sortieren
    [SerializeField] private string einheit = " m";
    [SerializeField] private int nachkommastellen = 2;
    [SerializeField] private string titel = "Leaderboard";

    private void Update()
    {
        if (text == null)
            return;

        if (playerDist == null || playerDist.Length == 0)
        {
            text.text = "Keine Daten";
            return;
        }

        // Sortierte Reihenfolge der Indizes bestimmen
        var order = groesserIstBesser
            ? Enumerable.Range(0, playerDist.Length).OrderByDescending(i => playerDist[i])
            : Enumerable.Range(0, playerDist.Length).OrderBy(i => playerDist[i]);

        // String zusammensetzen
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(titel))
            sb.AppendLine(titel);

        int rank = 1;
        foreach (var i in order)
        {
            float d = playerDist[i];
            string dStr = float.IsFinite(d)
                ? d.ToString("F" + nachkommastellen, CultureInfo.InvariantCulture)
                : "-";

            sb.AppendLine($"{rank}. Spieler {i + 1}: {dStr}{einheit}");
            rank++;
        }

        text.text = sb.ToString();
    }
}
