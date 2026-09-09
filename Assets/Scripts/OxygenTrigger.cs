using UnityEngine;
using System.Collections;
using Vuforia;

public class OxygenTrigger : MonoBehaviour
{
    [Header("Image Targets")]
    public ObserverBehaviour h1;
    public ObserverBehaviour h2;
    public ObserverBehaviour oxygenTarget;

    [Header("Prefabs")]
    public Transform h1Prefab;
    public Transform h2Prefab;
    public Transform oxygenPrefab;

    public GameObject h2oPrefab; // drag from Project window

    [Header("Electrons")]
    public Transform h1Electron;
    public Transform h2Electron;

    [Header("Bond Points")]
    public Transform bondPointH1;
    public Transform bondPointH2;

    [Header("Animation")]
    public float rotationDuration = 1.2f;
    public float transferDuration = 0.7f;

    [Header("Distance Settings")]
    public float bondDistance = 0.25f;
    public float breakDistance = 0.45f;
    public float stabilityTime = 0.8f;

    [Header("Molecule Spawn")]
    public float spawnDelay = 1f;

    private bool h1Bonded, h2Bonded;
    private bool moleculeFormed;
    private bool spawnStarted;

    private float h1Timer, h2Timer;

    private Vector3 h1OriginalLocal;
    private Vector3 h2OriginalLocal;

    private Coroutine h1Routine;
    private Coroutine h2Routine;

    private GameObject spawnedH2O;

    void Start()
    {
        h1OriginalLocal = h1Electron.localPosition;
        h2OriginalLocal = h2Electron.localPosition;
    }

    void Update()
    {
        HandleBondLogic(h1, h1Electron, h1Prefab, bondPointH1, ref h1Bonded, ref h1Timer, ref h1Routine, 1);
        HandleBondLogic(h2, h2Electron, h2Prefab, bondPointH2, ref h2Bonded, ref h2Timer, ref h2Routine, 2);

        // ---------- START MOLECULE SPAWN TIMER ----------
        if (h1Bonded && h2Bonded && !moleculeFormed && !spawnStarted)
        {
            spawnStarted = true;
            StartCoroutine(FormMoleculeAfterDelay());
        }

        // ---------- REVERT IF HYDROGEN MOVES AWAY ----------
        if (moleculeFormed)
        {
            float d1 = Vector3.Distance(h1.transform.position, oxygenTarget.transform.position);
            float d2 = Vector3.Distance(h2.transform.position, oxygenTarget.transform.position);

            if (d1 > breakDistance || d2 > breakDistance)
            {
                RevertToAtoms();
            }
        }
    }

    // ---------- TRACKING CHECK ----------
    bool IsTracked(ObserverBehaviour obs)
    {
        if (obs == null) return false;

        return obs.TargetStatus.Status == Status.TRACKED ||
               obs.TargetStatus.Status == Status.EXTENDED_TRACKED;
    }

    // ---------- BOND LOGIC ----------
    void HandleBondLogic(
        ObserverBehaviour hydrogenTarget,
        Transform electron,
        Transform hydrogenPrefab,
        Transform bondPoint,
        ref bool bonded,
        ref float timer,
        ref Coroutine routine,
        int id)
    {
        if (!IsTracked(hydrogenTarget) || !IsTracked(oxygenTarget))
            return;

        float distance = Vector3.Distance(
            hydrogenTarget.transform.position,
            oxygenTarget.transform.position
        );

        // FORM BOND
        if (!bonded && distance < bondDistance)
        {
            timer += Time.deltaTime;

            if (timer > stabilityTime)
            {
                if (routine != null) StopCoroutine(routine);
                routine = StartCoroutine(FormBond(electron, hydrogenPrefab, bondPoint));
                bonded = true;
                timer = 0;
            }
        }

        // BREAK BOND
        else if (bonded && distance > breakDistance)
        {
            timer += Time.deltaTime;

            if (timer > stabilityTime)
            {
                if (routine != null) StopCoroutine(routine);
                routine = StartCoroutine(BreakBond(electron, hydrogenPrefab, id));
                bonded = false;
                timer = 0;
            }
        }
        else
        {
            timer = 0;
        }
    }

    // ---------- FORM ELECTRON BOND ----------
    IEnumerator FormBond(Transform electron, Transform hydrogen, Transform bondPoint)
    {
        Vector3 startDir = (electron.position - hydrogen.position).normalized;
        Vector3 targetDir = (bondPoint.parent.position - hydrogen.position).normalized;
        float radius = (electron.position - hydrogen.position).magnitude;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / rotationDuration;
            Vector3 dir = Vector3.Slerp(startDir, targetDir, t);
            electron.position = hydrogen.position + dir * radius;
            yield return null;
        }

        Vector3 start = electron.position;
        Vector3 end = bondPoint.position;

        electron.SetParent(null);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / transferDuration;
            electron.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        electron.SetParent(bondPoint.parent);
    }

    // ---------- BREAK ELECTRON BOND ----------
    IEnumerator BreakBond(Transform electron, Transform hydrogen, int id)
    {
        Vector3 originalLocal = (id == 1) ? h1OriginalLocal : h2OriginalLocal;

        Vector3 start = electron.position;
        Vector3 end = hydrogen.TransformPoint(originalLocal);

        electron.SetParent(null);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / transferDuration;
            electron.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        electron.SetParent(hydrogen);
        electron.localPosition = originalLocal;
    }

    // ---------- DELAYED MOLECULE FORM ----------
    IEnumerator FormMoleculeAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        // cancel if bonds broke during delay
        if (!h1Bonded || !h2Bonded)
        {
            spawnStarted = false;
            yield break;
        }

        FormMolecule();
    }

    // ---------- FORM MOLECULE ----------
    void FormMolecule()
    {
        moleculeFormed = true;

        // hide original atoms
        h1Prefab.gameObject.SetActive(false);
        h2Prefab.gameObject.SetActive(false);
        oxygenPrefab.gameObject.SetActive(false);

        // spawn H2O
        spawnedH2O = Instantiate(
            h2oPrefab,
            oxygenPrefab.position,
            oxygenPrefab.rotation
        );
    }


    // ---------- REVERT ----------
    void RevertToAtoms()
    {
        moleculeFormed = false;
        spawnStarted = false;

        // remove H2O
        if (spawnedH2O != null)
            Destroy(spawnedH2O);

        // show original atoms
        h1Prefab.gameObject.SetActive(true);
        h2Prefab.gameObject.SetActive(true);
        oxygenPrefab.gameObject.SetActive(true);

        // reset electrons
        h1Electron.SetParent(h1Prefab);
        h2Electron.SetParent(h2Prefab);

        h1Electron.localPosition = h1OriginalLocal;
        h2Electron.localPosition = h2OriginalLocal;

        h1Bonded = false;
        h2Bonded = false;
        h1Timer = 0;
        h2Timer = 0;
    }

}
