
using System.Collections;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Bando")]
    public AIFloking boidPrefab;
    public int quantidade = 500;
    public float raioSpawn = 10f;

    [Header("Spawn gradual")]
    public float intervaloSpawn = 0.02f;
    public int quantidadePorLote = 1;

    private IEnumerator Start()
    {
        if (boidPrefab == null)
        {
            Debug.LogError("Arraste o prefab Boid para o campo Boid Prefab.");
            yield break;
        }

        int criados = 0;

        while (criados < quantidade)
        {
            for (int i = 0; i < quantidadePorLote && criados < quantidade; i++)
            {
                Vector3 posicao = transform.position +
                    Random.insideUnitSphere * raioSpawn;

                posicao.y = transform.position.y + Random.Range(1f, 5f);

                AIFloking boid = Instantiate(
                    boidPrefab,
                    posicao,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                );

                Rigidbody rb = boid.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 direcao = Random.insideUnitSphere;
                    direcao.y = 0f;

                    if (direcao.sqrMagnitude > 0.001f)
                        rb.linearVelocity = direcao.normalized * boid.speed;
                }

                criados++;
            }

            yield return new WaitForSeconds(intervaloSpawn);
        }

        Debug.Log("Todos os Boids foram criados: " + criados);
    }
}
