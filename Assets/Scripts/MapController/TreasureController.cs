using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TreasureController : MonoBehaviour
{
    public static Object[] fonts;

    [SerializeField] private TileBase closedTopTreasure;
    [SerializeField] private TileBase openTopTreasure;
    [SerializeField] private TileBase closedBottomTreasure;
    [SerializeField] private TileBase openBottomTreasure;

    [SerializeField] private GameObject goldGrab;
    [SerializeField] private GameObject _player;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Text scoreText;

    [SerializeField] public int newLevelIncrease;

    [SerializeField] private Color scoreAddColor;

    public static int score;
    public int scoreDelta;
    
    private Tilemap _tm;

    private void Awake()
    {
        _tm = GetComponent<Tilemap>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Vector3Int point = _tm.WorldToCell(collision.transform.position);

        if (_tm.GetTile(point) != null)
        {
            if (!_player.GetComponent<PlayerDamageController>().die)
            {
                if (_tm.GetTile(point).Equals(closedTopTreasure) || _tm.GetTile(point).Equals(closedBottomTreasure))
                {
                    Vector3 center = _tm.CellToWorld(point) + new Vector3(0.25f, 0.25f);
                    GameObject goldGrabInstance = Instantiate(goldGrab);
                    goldGrabInstance.transform.position = center;
                    goldGrabInstance.GetComponent<ParticleSystem>().Play();

                    //StartCoroutine(SFXController.instance.Play(SFX.OPEN_TREASURE, 1f));
                    SFXController.instance.Play(SFX.OPEN_TREASURE, 1f);

                    AddScore(scoreDelta);

                    Destroy(goldGrabInstance, 0.25f);
                }
                if (_tm.GetTile(point).Equals(closedTopTreasure))
                {
                    _tm.SetTile(point, openTopTreasure);
                }
                else if (_tm.GetTile(point).Equals(closedBottomTreasure))
                {
                    _tm.SetTile(point, openBottomTreasure);
                }
            }
        }
    }

    public void NewLevel()
    {
        AddScore(scoreDelta * 10);
        scoreDelta += newLevelIncrease;
        scoreText.text = score.ToString();
    }

    private void AddScore(int delta)
    {
        fonts = Resources.FindObjectsOfTypeAll(typeof(Font));
        Font scoreFont = null;

        foreach (Object font in fonts)
        {
            if (font.name == "GamegirlClassic-9MVj")
            {
                scoreFont = (Font)font;
                break;
            }
        }
        
        GameObject addScoreGameObject = new GameObject("AddScoreText", typeof(RectTransform));

        addScoreGameObject.AddComponent<CanvasRenderer>();
        Text scoreDeltaText = addScoreGameObject.AddComponent<Text>();

        RectTransform addScoreTransform = addScoreGameObject.GetComponent<RectTransform>();

        addScoreTransform.SetParent(canvas.transform);

        addScoreTransform.anchoredPosition = scoreText.GetComponent<RectTransform>().anchoredPosition + new Vector2(0f, 5f);
        addScoreTransform.sizeDelta = new Vector2(125f, 30f);
        addScoreTransform.localScale = Vector3.one;
        addScoreTransform.gameObject.layer = 5;

        scoreDeltaText.font = scoreFont;
        scoreDeltaText.fontSize = 15;
        scoreDeltaText.color = scoreAddColor;

        scoreDeltaText.alignment = TextAnchor.MiddleCenter;
        scoreDeltaText.horizontalOverflow = HorizontalWrapMode.Overflow;
        scoreDeltaText.verticalOverflow = VerticalWrapMode.Overflow;

        scoreDeltaText.text = "+" + delta;

        StartCoroutine(RemoveText(addScoreTransform));

        score += delta;
        scoreText.text = score.ToString();
    }

    IEnumerator RemoveText(RectTransform addScoreTransform)
    {
        for (int y = 0; y < 10; y++)
        {
            addScoreTransform.anchoredPosition += new Vector2(0f, y * 0.5f);
            addScoreTransform.GetComponent<Text>().color = new Color(addScoreTransform.GetComponent<Text>().color.r, addScoreTransform.GetComponent<Text>().color.g, addScoreTransform.GetComponent<Text>().color.b, addScoreTransform.GetComponent<Text>().color.a - 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(addScoreTransform.gameObject);
    }
}
