using UnityEngine;

// Shows a big title banner when the level starts.
// Same style as Raina's Surface Level layer banners (gold title, white subtitle, dark outline).
public class LevelBanner : MonoBehaviour
{
    [Header("Text")]
    public string title = "The Asteroid Belt";
    public string subtitle = "Dodge the rocks and watch your fuel!";

    [Header("Timing")]
    public float startDelay = 0.5f;        // wait before showing (seconds)
    public float showTime = 3f;            // how long it stays up
    public float fadeTime = 0.4f;          // fade in/out. 0 = pop on/off like Raina's

    [Header("Style")]
    public Color titleColor = new Color(1f, 0.9f, 0.4f);
    public Color subtitleColor = Color.white;
    public Color shadowColor = new Color(0.1f, 0.05f, 0.15f, 0.9f);
    public int titleSize = 44;
    public int subtitleSize = 22;
    [Range(0f, 1f)] public float heightOnScreen = 0.18f;   // 0 = top of screen, 1 = bottom

    string curTitle, curSub;
    float showStart = -999f;
    GUIStyle big, sub;

    void Start()
    {
        Show(title, subtitle, startDelay);
    }

    // Call this from other scripts to show a new banner later, e.g. FindObjectOfType<LevelBanner>().Show("Meteor Storm!", "Hold on tight");
    public void Show(string newTitle, string newSubtitle, float delay = 0f)
    {
        curTitle = newTitle;
        curSub = newSubtitle;
        showStart = Time.time + delay;
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(curTitle)) return;

        float t = Time.time - showStart;
        if (t < 0f || t > showTime) return;

        float alpha = 1f;
        if (fadeTime > 0f)
            alpha = Mathf.Min(Mathf.Clamp01(t / fadeTime), Mathf.Clamp01((showTime - t) / fadeTime));

        if (big == null)
        {
            big = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            sub = new GUIStyle(big);
        }
        big.fontSize = titleSize;
        sub.fontSize = subtitleSize;

        float y = Screen.height * heightOnScreen;
        float titleH = titleSize * 1.4f;

        Outlined(new Rect(0, y, Screen.width, titleH), curTitle, big, titleColor, alpha);
        if (!string.IsNullOrEmpty(curSub))
            Outlined(new Rect(0, y + titleSize * 1.27f, Screen.width, subtitleSize * 1.6f), curSub, sub, subtitleColor, alpha);
    }

    void Outlined(Rect r, string text, GUIStyle style, Color color, float alpha)
    {
        Color shadow = shadowColor;
        shadow.a *= alpha;
        style.normal.textColor = shadow;
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, style);

        color.a *= alpha;
        style.normal.textColor = color;
        GUI.Label(r, text, style);
    }
}