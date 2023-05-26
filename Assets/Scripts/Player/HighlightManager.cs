using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class HighlightManager : NetworkBehaviour
{

    private Transform highlightedObj;
    private Transform selectedObj;
    public LayerMask selectableLayer;

    private Outline highlightOutline;
    private RaycastHit hit;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return;  }
        HoverHighlight();   
    }

    public void HoverHighlight()
    {
        if (highlightedObj != null)
        {
            highlightOutline.enabled = false;
            highlightedObj = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, selectableLayer))
        {
            highlightedObj = hit.transform;
            // TODO: Or "Enemy" for monsters
            if (highlightedObj.gameObject != gameObject && highlightedObj.CompareTag("Player") && highlightedObj != selectedObj)
            {
                highlightOutline = highlightedObj.GetComponent<Outline>();
                highlightOutline.enabled = true;
            }
            else
            {
                highlightedObj = null;
            }
        }
    }

    public void SelectedHighlight()
    {
        // TODO: Or "Enemy" for monsters
        if (highlightedObj != null && highlightedObj.CompareTag("Player"))
        {
            if (selectedObj != null)
            {
                selectedObj.GetComponent<Outline>().enabled = false;
            }

            selectedObj = hit.transform;
            selectedObj.GetComponent<Outline>().enabled = true;

            highlightOutline.enabled = true;
            highlightedObj = null;
        }
    }

    public void DeselectHighlight()
    {
        if (selectedObj == null) {return; }
        selectedObj.GetComponent<Outline>().enabled = false;
        selectedObj = null;
    }
}
