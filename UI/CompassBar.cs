/* using UnityEngine;

public class CompassBar : MonoBehaviour
{
    public Transform viewDirection;              // Player facing direction
    public RectTransform compassContainer;       // UI container for compass
    public float visibleAngleRange = 90f;         // Visibility cutoff in degrees
    public RectTransform[] compassElements;      // Compass markers

    [Header("Dynamic Marker Settings")]
    public RectTransform questMarkerUI;          // UI icon for quest marker
    public GameObject activeStepWorldObject;     // The world waypoint

    void LateUpdate()
    {
        if (!viewDirection || !compassContainer) return;

        // Update reference to activeStepWorldObject
        UpdateActiveStep();

        compassContainer.gameObject.SetActive(true);

        // Player forward vector (flipped 180� for correct compass rotation)
        Vector3 forwardVector = Vector3.ProjectOnPlane(viewDirection.forward, Vector3.up).normalized;
        float forwardSignedAngle = Vector3.SignedAngle(Vector3.forward, forwardVector, Vector3.up) + 180f;
        forwardSignedAngle = Mathf.Repeat(forwardSignedAngle + 180f, 360f) - 180f;

        // Static compass elements (N,E,S,W)
        foreach (var element in compassElements)
        {
            UpdateCompassElement(element, forwardSignedAngle);
        }

        // Dynamic quest marker
        if (questMarkerUI != null)
        {
            UpdateQuestMarker(forwardSignedAngle);
        }
    }


    void UpdateCompassElement(RectTransform element, float forwardSignedAngle)
    {
        if (!element) return;

        float elementAngle = GetElementAngle(element.name);

        float angleOffset = elementAngle - forwardSignedAngle;
        angleOffset = Mathf.Repeat(angleOffset + 180f, 360f) - 180f;

        float normalizedOffset = (angleOffset / 180f) * compassContainer.rect.width;
        element.anchoredPosition = new Vector3(normalizedOffset, 0f);

        element.gameObject.SetActive(Mathf.Abs(angleOffset) <= visibleAngleRange);
    }

    void UpdateQuestMarker(float forwardSignedAngle)
    {
        if (activeStepWorldObject == null)
        {
            questMarkerUI.gameObject.SetActive(false);
            return;
        }

        Vector3 dirToTarget = activeStepWorldObject.transform.position - viewDirection.position;
        dirToTarget.y = 0; // Ignore vertical difference
        dirToTarget.Normalize();

        // Angle from player's forward (Vector3.forward) to target (flipped like in LateUpdate)
        float targetAngle = Vector3.SignedAngle(Vector3.forward, dirToTarget, Vector3.up) + 180f;
        targetAngle = Mathf.Repeat(targetAngle + 180f, 360f) - 180f;

        // Offset relative to current facing
        float angleOffset = targetAngle - forwardSignedAngle;
        angleOffset = Mathf.Repeat(angleOffset + 180f, 360f) - 180f;

        // Position marker
        float normalizedOffset = (angleOffset / 180f) * compassContainer.rect.width;
        questMarkerUI.anchoredPosition = new Vector3(normalizedOffset, 0f);

        // Show only if within visible range
        questMarkerUI.gameObject.SetActive(Mathf.Abs(angleOffset) <= visibleAngleRange);
    }


    void UpdateActiveStep()
    {
        activeStepWorldObject = null;

        if (QuestSystem.Instance != null && QuestSystem.Instance.currentQuest != null)
        {
            Quest current = QuestSystem.Instance.currentQuest;

            if (current.isCurrentQuest)
            {
                foreach (var part in current.Parts)
                {
                    if (part != null && part.isActive)
                    {
                        if (part is Waypoint waypointScript)
                        {
                            // Access private spawnedWaypoint from Waypoint
                            var spawnedField = waypointScript.GetType()
                                .GetField("spawnedWaypoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            if (spawnedField != null)
                            {
                                GameObject waypointObj = spawnedField.GetValue(waypointScript) as GameObject;
                                if (waypointObj != null)
                                    activeStepWorldObject = waypointObj;
                            }
                        }
                        else if (part is Hunt huntScript)
                        {
                            if (huntScript.SpawnedMarker != null)
                                activeStepWorldObject = huntScript.SpawnedMarker;
                        }

                        else if (part is TalkToNPC talkScript)
                        {
                            if (talkScript.SpawnedMarker != null)
                                activeStepWorldObject = talkScript.SpawnedMarker;
                        }

                        // Stop at the first active step
                        if (activeStepWorldObject != null)
                            break;
                    }
                }
            }
        }
    }


    float GetElementAngle(string elementName)
    {
        switch (elementName)
        {
            case "North": return 0f;
            case "NNE": return 22.5f;
            case "NE": return 45f;
            case "NEE": return 67.5f;
            case "East": return 90f;
            case "SEE": return 112.5f;
            case "SE": return 135f;
            case "SSE": return 157.5f;
            case "South": return 180f;
            case "South 2": return -180f;
            case "SSW": return -157.5f;
            case "SW": return -135f;
            case "SWW": return -112.5f;
            case "West": return -90f;
            case "NWW": return -67.5f;
            case "NW": return -45f;
            case "NNW": return -22.5f;
            default: return 0f;
        }
    }
}
*/