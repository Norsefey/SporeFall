using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StanleyLevel : StructureLevel
{
    [Header("Stanley Specific")]
    [Tooltip("How often to give mycelia")]
    public float myceliaGenerationTickRate;
    [Tooltip("How Much mycelia to give")]
    public float myceliaGenerationRate;

    public float moveSpeed = 2f;
    public float turnSpeed = 1f;    // How fast the object changes direction
    public float changeDirectionInterval = 2f;    // How often to change direction
    public float detectionRadius = 10f;    // Detection radius for nearby Mycelia
    public float randomMovementWeight = 1f;    // Weight for random movement (higher = more random movement)

}
