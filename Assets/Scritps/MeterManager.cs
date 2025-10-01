using UnityEngine;
using System.Collections.Generic;

public class MeterManager : MonoBehaviour
{
    [System.Serializable]
    public class Meter
    {
        public Transform needle;           // El palo negro del medidor
        public float maxAngle = 90f;      // Ángulo máximo de rotación (puede ser negativo)
        public float minSpeed = 10f;      // Velocidad mínima de rotación
        public float maxSpeed = 50f;      // Velocidad máxima de rotación
        public float returnSpeed = 15f;   // Velocidad de retorno a cero

        [HideInInspector]
        public float currentSpeed;        // Velocidad actual de rotación
        [HideInInspector]
        public float currentAngle = 0f;   // Ángulo actual de rotación
        [HideInInspector]
        public bool returning = false;    // Si está regresando a cero
    }

    public List<Meter> meters = new List<Meter>(); // Lista de medidores

    void Start()
    {
        // Inicializar cada medidor con una velocidad aleatoria
        foreach (Meter meter in meters)
        {
            if (meter.needle != null)
            {
                // Determinar la dirección inicial basada en el maxAngle
                if (meter.maxAngle >= 0)
                {
                    meter.currentSpeed = Random.Range(meter.minSpeed, meter.maxSpeed);
                }
                else
                {
                    meter.currentSpeed = -Random.Range(meter.minSpeed, meter.maxSpeed);
                }

                meter.currentAngle = 0f;
                meter.returning = false;
                meter.needle.rotation = Quaternion.Euler(0, 0, meter.currentAngle);
            }
        }
    }

    void Update()
    {
        // Rotar cada medidor según su estado
        foreach (Meter meter in meters)
        {
            if (meter.needle == null) continue;

            if (!meter.returning)
            {
                // Movimiento hacia el objetivo
                meter.currentAngle += meter.currentSpeed * Time.deltaTime;

                // Comprobar si ha alcanzado el límite (dependiendo de la dirección)
                if (meter.maxAngle >= 0)
                {
                    // Para ángulos positivos
                    if (meter.currentAngle >= meter.maxAngle)
                    {
                        meter.currentAngle = meter.maxAngle;
                        meter.returning = true;
                    }
                }
                else
                {
                    // Para ángulos negativos
                    if (meter.currentAngle <= meter.maxAngle)
                    {
                        meter.currentAngle = meter.maxAngle;
                        meter.returning = true;
                    }
                }
            }
            else
            {
                // Movimiento de retorno a cero
                if (meter.maxAngle >= 0)
                {
                    // Retorno desde ángulo positivo
                    meter.currentAngle -= meter.returnSpeed * Time.deltaTime;
                    if (meter.currentAngle <= 0f)
                    {
                        meter.currentAngle = 0f;
                        meter.returning = false;
                        meter.currentSpeed = Random.Range(meter.minSpeed, meter.maxSpeed);
                    }
                }
                else
                {
                    // Retorno desde ángulo negativo
                    meter.currentAngle += meter.returnSpeed * Time.deltaTime;
                    if (meter.currentAngle >= 0f)
                    {
                        meter.currentAngle = 0f;
                        meter.returning = false;
                        meter.currentSpeed = -Random.Range(meter.minSpeed, meter.maxSpeed);
                    }
                }
            }

            // Aplicar la rotación
            meter.needle.rotation = Quaternion.Euler(0, 0, meter.currentAngle);
        }
    }

    // Método para reiniciar todos los medidores
    public void ResetAllMeters()
    {
        foreach (Meter meter in meters)
        {
            if (meter.needle != null)
            {
                meter.currentAngle = 0f;
                meter.returning = false;

                // Determinar la dirección basada en el maxAngle
                if (meter.maxAngle >= 0)
                {
                    meter.currentSpeed = Random.Range(meter.minSpeed, meter.maxSpeed);
                }
                else
                {
                    meter.currentSpeed = -Random.Range(meter.minSpeed, meter.maxSpeed);
                }

                meter.needle.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    // Método para cambiar las velocidades aleatoriamente
    public void RandomizeSpeeds()
    {
        foreach (Meter meter in meters)
        {
            if (meter.needle != null && !meter.returning)
            {
                if (meter.maxAngle >= 0)
                {
                    meter.currentSpeed = Random.Range(meter.minSpeed, meter.maxSpeed);
                }
                else
                {
                    meter.currentSpeed = -Random.Range(meter.minSpeed, meter.maxSpeed);
                }
            }
        }
    }
}