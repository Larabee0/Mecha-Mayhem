using RedButton.Core;
using RedButton.Core.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenBootStrapper : MonoBehaviour
{
    [SerializeField] private ControlArbiter controlArbiterPrefab;
    [SerializeField] private MainUIController mainUIControllerPrefab;

    private void Awake()
    {
        if(ControlArbiter.Instance == null)
        {
            Instantiate(mainUIControllerPrefab);
            Instantiate(controlArbiterPrefab);
        }
        Destroy(gameObject);
    }
}
