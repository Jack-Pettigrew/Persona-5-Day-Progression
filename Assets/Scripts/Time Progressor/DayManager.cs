using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Tooltip("Ensure days are added in order - old to new dates")]
    [SerializeField] private TextMeshProUGUI[] dayTexts;

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < dayTexts.Length; i++)
        {
            dayTexts[i].text = (i + 12).ToString();
        }
    }
}
