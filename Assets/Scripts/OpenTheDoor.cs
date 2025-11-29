using System;
using System.Collections;
using UnityEngine;

public class OpenTheDoor : MonoBehaviour
{
    public Transform theDoor;

    public float rotationSpeed = 90f; // grados por segundo

    bool doorOpened;

    bool doorCanBeOpened;

    bool doorCanBeClosed;


    private void Update()
    {
        if (doorCanBeOpened && Input.GetKey(KeyCode.E))
        {
            StartRotationOpen();

            doorCanBeOpened = false;
            doorCanBeClosed = true;
        }

        if (doorCanBeClosed && Input.GetKey(KeyCode.E))
        {
            StartRotationClose();

            doorCanBeOpened = true;
            doorCanBeClosed = false;
        }


    }
    // Llama a esta función para iniciar la rotación
    public void StartRotationOpen()
    {       

        StartCoroutine(RotateToAngle(-90f));

        doorOpened = true;
        
    }

    public void StartRotationClose()
    {       
        StartCoroutine(RotateToAngle(0));

        doorOpened = false;

    }

    private IEnumerator RotateToAngle(float targetAngle)
    {
        // Ángulo inicial
        float currentY = theDoor.transform.eulerAngles.y;
        // Ajustamos para que siempre rote en la dirección más corta
        float angle = Mathf.DeltaAngle(currentY, targetAngle);

        while (Mathf.Abs(angle) > 0.1f)
        {
            float step = rotationSpeed * Time.deltaTime;
            float newY = Mathf.MoveTowardsAngle(theDoor.transform.eulerAngles.y, targetAngle, step);
            theDoor.transform.rotation = Quaternion.Euler(theDoor.transform.eulerAngles.x, newY, theDoor.transform.eulerAngles.z);

            // Recalcular el delta restante
            angle = Mathf.DeltaAngle(theDoor.transform.eulerAngles.y, targetAngle);

            yield return null; // Espera un frame
        }

        // Asegurar que quede exactamente en el ángulo deseado
        theDoor.transform.rotation = Quaternion.Euler(theDoor.transform.eulerAngles.x, targetAngle, theDoor.transform.eulerAngles.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Contains("Player"))
        {
            doorCanBeOpened = true;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Contains("Player"))
        {
            doorCanBeOpened = false;

        }
    }
}
