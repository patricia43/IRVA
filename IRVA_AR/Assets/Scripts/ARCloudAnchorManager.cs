using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using TMPro;

public class AnchorCreatedEvent : UnityEvent<Transform> { }

/* TODO 1. Add and configure the ARCore Extensions game object */
/* TODO 2. Enable ARCore Cloud Anchors API on Google Cloud Platform */
public class ARCloudAnchorManager : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    TMP_Text statusUpdate;

    private ARAnchorManager  arAnchorManager = null;
    private ARAnchor pendingHostAnchor = null;
    private string anchorIdToResolve;
    private AnchorCreatedEvent anchorCreatedEvent = null;
    public static ARCloudAnchorManager Instance { get; private set; }
    public GameObject middle;
    public GameObject main;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        anchorCreatedEvent = new AnchorCreatedEvent();
        anchorCreatedEvent.AddListener((t) => CloudAnchorObjectPlacement.Instance.RecreatePlacement(t));

        arAnchorManager = GetComponent<ARAnchorManager>();
    }

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor = arAnchor;
    }

    public IEnumerator DisplayStatus(string text)
    {
        statusUpdate.text = text;
        yield return new WaitForSeconds(3);
        statusUpdate.text = "";
    }

    public void Host()
    {
        /* TODO 4.1 Get FeatureMapQuality */
        FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(new Pose(arCamera.transform.position, arCamera.transform.rotation));
        StartCoroutine(DisplayStatus("HostAnchor call in progress. Feature Map Quality: " + quality));

        //if (quality != FeatureMapQuality.Insufficient)
        //{
            /* TODO 4.2 Start the hosting process */
            HostCloudAnchorPromise cloudAnchor = arAnchorManager.HostCloudAnchorAsync(pendingHostAnchor, 100);

            /* Wait for the promise to solve (Hint! Pass the HostCloudAnchorPromise variable to the coroutine) */
            StartCoroutine(WaitHostingResult(cloudAnchor));
        //}
    }

    public void Resolve()
    {
        StartCoroutine(DisplayStatus("Resolve call in progress"));

        /* TODO 6 Start the resolve process and wait for the promise */
        ResolveCloudAnchorPromise resolvePromise = arAnchorManager.ResolveCloudAnchorAsync(anchorIdToResolve);

        StartCoroutine(WaitResolvingResult(resolvePromise));

    }

    private IEnumerator WaitHostingResult(HostCloudAnchorPromise hostingPromise)
    {
        /* TODO 4.3 Wait for the promise. Save the id if the hosting succeeded */
        /* Wait for the promise. Save the id if the hosting succeeded */
        yield return hostingPromise;

        if (hostingPromise.State == PromiseState.Cancelled)
        {
            yield break;
        }

        var result = hostingPromise.Result;

        if (result.CloudAnchorState == CloudAnchorState.Success)
        {
            anchorIdToResolve = result.CloudAnchorId;
            Debug.Log("Anchor hosted successfully!");
            StartCoroutine(DisplayStatus("Anchor hosted successfully!"));
        }
        else
        {
            Debug.Log(string.Format("Error in hosting the anchor: {0}", result.CloudAnchorState));
            StartCoroutine(DisplayStatus(string.Format("Error in hosting the anchor: {0}", result.CloudAnchorState)));
        }
    }

    private IEnumerator WaitResolvingResult(ResolveCloudAnchorPromise resolvePromise)
    {
        yield return resolvePromise;

        if (resolvePromise.State == PromiseState.Cancelled) yield break;
        var result = resolvePromise.Result;

        if (result.CloudAnchorState == CloudAnchorState.Success)
        {
            anchorCreatedEvent?.Invoke(result.Anchor.transform);
            StartCoroutine(DisplayStatus("Anchor resolved successfully!"));
        }
        else
        {
            StartCoroutine(DisplayStatus("Error while resolving cloud anchor"));
        }
    }
}
