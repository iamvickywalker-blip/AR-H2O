using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [Header("Marker Settings")]
    public float attractionRange = 5f;
    public string hydrogenTag = "Hydrogen";
    public string oxygenTag = "Oxygen";

    [Header("Follow Settings")]
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0.5f, 0, 0);

    [Header("Original Positions")]
    public GameObject hydrogenOriginalPosition1;
    public GameObject hydrogenOriginalPosition2;
    public GameObject oxygenOriginalPosition;

    public List<Transform> hydrogenMarkers = new List<Transform>();
    private Transform oxygenMarker;
    private Vector3 velocity1 = Vector3.zero;
    private Vector3 velocity2 = Vector3.zero;
    private Vector3 oxygenVelocity = Vector3.zero;
    public GameObject h2O;

    private bool markersAreActive = false;

    private void Start()
    {
        // Find and add the hydrogen markers
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(hydrogenTag))
        {
            hydrogenMarkers.Add(obj.transform);
        }

        // Find the oxygen marker
        GameObject oxygenObj = GameObject.FindGameObjectWithTag(oxygenTag);
        if (oxygenObj != null)
        {
            oxygenMarker = oxygenObj.transform;
        }
        else
        {
            Debug.LogError("Oxygen marker not found. Please ensure an object with the 'Oxygen' tag exists.");
        }

        // Set H2O object to inactive initially
        h2O.SetActive(false);
        // Set markers to inactive initially
        SetMarkersActive(false);
    }

    private void FixedUpdate()
    {
        if (oxygenMarker == null || hydrogenMarkers.Count == 0) return;

        bool allInRange = true;

        foreach (Transform hydrogen in hydrogenMarkers)
        {
            if (Vector3.Distance(hydrogen.position, oxygenMarker.position) > attractionRange)
            {
                allInRange = false;
                break;
            }
        }

        bool hydrogen1OutOfRange = hydrogenMarkers.Count > 0 && Vector3.Distance(hydrogenMarkers[0].position, hydrogenOriginalPosition1.transform.position) > attractionRange;
        bool hydrogen2OutOfRange = hydrogenMarkers.Count > 1 && Vector3.Distance(hydrogenMarkers[1].position, hydrogenOriginalPosition2.transform.position) > attractionRange;
        bool oxygenOutOfRange = Vector3.Distance(oxygenMarker.position, oxygenOriginalPosition.transform.position) > attractionRange;

        bool shouldBeActive = !(allInRange && !hydrogen1OutOfRange && !hydrogen2OutOfRange && !oxygenOutOfRange);

        if (allInRange && !hydrogen1OutOfRange && !hydrogen2OutOfRange && !oxygenOutOfRange)
        {
            if (hydrogenMarkers.Count > 0)
            {
                hydrogenMarkers[0].position = Vector3.SmoothDamp(hydrogenMarkers[0].position, oxygenMarker.position + offset, ref velocity1, followSpeed * Time.deltaTime);
            }
            if (hydrogenMarkers.Count > 1)
            {
                hydrogenMarkers[1].position = Vector3.SmoothDamp(hydrogenMarkers[1].position, oxygenMarker.position - offset, ref velocity2, followSpeed * Time.deltaTime);
            }

            Debug.Log("Markers forming H2O structure");

            h2O.SetActive(true);
        }
        else
        {
            if (hydrogenMarkers.Count > 0)
            {
                hydrogenMarkers[0].position = Vector3.SmoothDamp(hydrogenMarkers[0].position, hydrogenOriginalPosition1.transform.position, ref velocity1, followSpeed * Time.deltaTime);
            }
            if (hydrogenMarkers.Count > 1)
            {
                hydrogenMarkers[1].position = Vector3.SmoothDamp(hydrogenMarkers[1].position, hydrogenOriginalPosition2.transform.position, ref velocity2, followSpeed * Time.deltaTime);
            }

            if (oxygenMarker != null)
            {
                oxygenMarker.position = Vector3.SmoothDamp(oxygenMarker.position, oxygenOriginalPosition.transform.position, ref oxygenVelocity, followSpeed * Time.deltaTime);
            }

            h2O.SetActive(false);

            Debug.Log("Markers out of range, returning to original positions");
        }

        if (markersAreActive != shouldBeActive)
        {
            SetMarkersActive(shouldBeActive);
            markersAreActive = shouldBeActive;
        }
    }

    private void SetMarkersActive(bool isActive)
    {
        foreach (Transform hydrogen in hydrogenMarkers)
        {
            hydrogen.gameObject.SetActive(isActive);
        }

        if (oxygenMarker != null)
        {
            oxygenMarker.gameObject.SetActive(isActive);
        }
    }

    public void SetActiveBonds(GameObject Bonds)
    {
        if (!h2O.activeSelf)
        {
            Bonds.SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (oxygenMarker == null)
        {
            GameObject oxygenObj = GameObject.FindGameObjectWithTag(oxygenTag);
            if (oxygenObj != null)
            {
                oxygenMarker = oxygenObj.transform;
            }
        }

        if (oxygenMarker != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(oxygenMarker.position, attractionRange);
        }
    }
}