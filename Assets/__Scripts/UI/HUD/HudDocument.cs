using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class HudDocument : MonoBehaviour
{
    private static bool _documentActive = false;
    public static bool DocumentActive
    {
        get => _documentActive;
        private set
        {
            _documentActive = value;

            if(_documentActive)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            OnDocumentUpdated?.Invoke(_documentActive);
        }
    }

    public static event Action<bool> OnDocumentUpdated;

    private static event Action<TextAsset> OnDocumentOpened;

    [Header("Components")]
    [SerializeField] private RectTransform documentTransform;
    [SerializeField] private GameObject closePrompt;
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("Animation")]
    [SerializeField] private float animationTime = 1f;

    [Space]
    [SerializeField] private Vector2 hiddenPosition = Vector2.zero;
    [SerializeField] private float maxHiddenRotation = 10f;

    [Space]
    [SerializeField] private Vector2 shownPosition = Vector2.zero;
    [SerializeField] private float maxShownRotation = 3f;

    private bool animating;
    private Coroutine animationCoroutine;


    public static void OpenDocument(TextAsset documentText)
    {
        OnDocumentOpened?.Invoke(documentText);
    }


    private static float RandomRotation(float max)
    {
        return UnityEngine.Random.Range(-max, max);
    }


    private IEnumerator ShowDocumentCoroutine(Vector2 startPos, float startRotation)
    {
        animating = true;

        float targetRotation = RandomRotation(maxShownRotation);

        float t = 0f;
        while(t < 1f)
        {
            float positionTransition = Easings.Quad.Out(t);
            float rotationTransition = Easings.Sine.Out(t);

            float angle = Mathf.Lerp(startRotation, targetRotation, rotationTransition);
            documentTransform.anchoredPosition = Vector2.Lerp(startPos, shownPosition, positionTransition);
            documentTransform.localEulerAngles = new Vector3(0f, 0f, angle);

            t += Time.deltaTime / animationTime;
            yield return null;
        }

        documentTransform.anchoredPosition = shownPosition;
        documentTransform.localEulerAngles = new Vector3(0f, 0f, targetRotation);

        animating = false;
    }


    private IEnumerator HideDocumentCoroutine(Vector2 startPos, float startRotation)
    {
        animating = true;

        float targetRotation = RandomRotation(maxHiddenRotation);

        float t = 0f;
        while(t < 1f)
        {
            float positionTransition = Easings.Quad.In(t);
            float rotationTransition = Easings.Sine.In(t);

            float angle = Mathf.Lerp(startRotation, targetRotation, rotationTransition);
            documentTransform.anchoredPosition = Vector2.Lerp(startPos, hiddenPosition, positionTransition);
            documentTransform.localEulerAngles = new Vector3(0f, 0f, angle);

            t += Time.deltaTime / animationTime;
            yield return null;
        }

        documentTransform.anchoredPosition = hiddenPosition;
        documentTransform.localEulerAngles = new Vector3(0f, 0f, targetRotation);

        documentTransform.gameObject.SetActive(false);

        animating = false;
    }


    public void ShowDocument(TextAsset documentText, bool animate = true)
    {
        documentTransform.gameObject.SetActive(true);
        closePrompt.SetActive(true);
        textMesh.text = documentText.text;

        DocumentActive = true;

        if(animating)
        {
            StopCoroutine(animationCoroutine);
        }

        if(animate)
        {
            Vector2 startPos = documentTransform.anchoredPosition;
            float startRotation = documentTransform.eulerAngles.z;

            if(startRotation > 180f)
            {
                startRotation -= 360f;
            }

            animationCoroutine = StartCoroutine(ShowDocumentCoroutine(startPos, startRotation));
        }
        else
        {
            documentTransform.anchoredPosition = shownPosition;
            documentTransform.localEulerAngles = new Vector3(0f, 0f, RandomRotation(maxShownRotation));
        }
    }


    public void HideDocument(bool animate = true)
    {
        DocumentActive = false;
        closePrompt.SetActive(false);

        if(animating)
        {
            StopCoroutine(animationCoroutine);
        }

        if(animate)
        {
            Vector2 startPos = documentTransform.anchoredPosition;
            float startRotation = documentTransform.eulerAngles.z;

            if(startRotation > 180f)
            {
                startRotation -= 360f;
            }

            animationCoroutine = StartCoroutine(HideDocumentCoroutine(startPos, startRotation));
        }
        else
        {
            documentTransform.anchoredPosition = hiddenPosition;
            documentTransform.localEulerAngles = new Vector3(0f, 0f, RandomRotation(maxHiddenRotation));
            documentTransform.gameObject.SetActive(false);
        }
    }


    private void Start()
    {
        if(!DocumentActive)
        {
            HideDocument(false);
        }

        OnDocumentOpened += (TextAsset textAsset) => ShowDocument(textAsset);
    }

    
    private void Update()
    {
        if(DocumentActive && Input.GetButtonDown("ExitTerminal"))
        {
            HideDocument();
        }
    }
}