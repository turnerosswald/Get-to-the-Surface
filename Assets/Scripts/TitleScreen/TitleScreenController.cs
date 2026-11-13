using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Title screen flow: blinking "PRESS SPACE TO START" -> on Space/Enter/click the Rock Elemental
// pops out of the lava crater, zooms up to fill the screen, fades to black, then loads the game.
public class TitleScreenController : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string gameScene = "SampleScene";

    [Header("Pieces")]
    [SerializeField] private SpriteRenderer pressStart;
    [SerializeField] private Sprite pressStartNormal;
    [SerializeField] private Sprite pressStartHot;
    [SerializeField] private SpriteRenderer hero;
    [SerializeField] private SpriteRenderer fade;
    [SerializeField] private SpriteRenderer loading;
    [SerializeField] private Vector2 craterPx = new Vector2(172, 118);

    private bool started;

    public void Setup(SpriteRenderer press, Sprite normal, Sprite hot, SpriteRenderer heroSr,
        SpriteRenderer fadeSr, SpriteRenderer loadingSr, Vector2 crater)
    {
        pressStart = press; pressStartNormal = normal; pressStartHot = hot;
        hero = heroSr; fade = fadeSr; loading = loadingSr; craterPx = crater;
    }

    private void Start()
    {
        hero.enabled = false;
        loading.enabled = false;
        SetFade(0f);
    }

    private void Update()
    {
        if (started) return;
        pressStart.enabled = Mathf.FloorToInt(Time.time * 2f) % 2 == 0;

        bool pressed =
            (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (pressed) StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        started = true;

        // 1) the prompt flashes yellow
        pressStart.sprite = pressStartHot;
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            pressStart.enabled = Mathf.FloorToInt(t * 12f) % 2 == 0;
            yield return null;
        }
        pressStart.enabled = false;

        // 2) the Rock Elemental pops out of the crater
        hero.enabled = true;
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            float u = t / 1f;
            float pop = 1f + 0.25f * Mathf.Sin(u * Mathf.PI);
            SetHero(craterPx.x, craterPx.y - u * 14f, 2f * Mathf.Min(1f, u * 2.2f) * pop);
            yield return null;
        }

        // 3) it zooms up to fill the whole screen while the background darkens
        Vector2 from = new Vector2(craterPx.x, craterPx.y - 14f);
        Vector2 to = new Vector2(TitlePixels.Width / 2f, TitlePixels.Height / 2f + 20f);
        for (float t = 0; t < 1.2f; t += Time.deltaTime)
        {
            float u = t / 1.2f;
            float e = u * u * (3f - 2f * u);
            Vector2 p = Vector2.Lerp(from, to, e);
            SetHero(p.x, p.y, 2.25f + e * e * 14f);
            SetFade(0.5f * e);
            yield return null;
        }

        // 4) fade to black, show "LOADING LEVEL..."
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            SetFade(0.5f + 0.5f * (t / 0.5f));
            yield return null;
        }
        SetFade(1f);
        loading.enabled = true;
        yield return new WaitForSeconds(0.6f);

        SceneManager.LoadScene(gameScene);
    }

    private void SetHero(float px, float py, float scale)
    {
        hero.transform.position = TitlePixels.ToWorld(px, py);
        hero.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void SetFade(float a)
    {
        Color c = fade.color; c.a = a; fade.color = c;
    }
}
