using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameInputManger : MonoBehaviour
{
    string m_name = "";

    public void OnInputChange(string value) {
        m_name = value;
    }

    public void OnConfirm() {
        Debug.Log($"Name is \"{m_name}\"");

        // Make sure the name is not empty
        if (m_name == "") {
            return;
        }

        // Start the game!
        Debug.Log("All set, let's start the game!");
        // TODO Init a new game state, set the name, trigger the @@START@@ event
    }
}
