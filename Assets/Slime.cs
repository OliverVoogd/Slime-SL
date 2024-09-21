using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Agent {
    public Vector2 position;
    public float angle;
}
public class Slime : MonoBehaviour
{
    public ComputeShader slimeShader;

    private RenderTexture TrailMap;
    [SerializeField]
    int height = 384, width = 512;
    [SerializeField]
    int numAgents = 1000;
    [SerializeField]
    float moveSpeed = 5.0f;
    [SerializeField]
    float spawnRadius = 10.0f;
    [SerializeField]
    float decayRate = 0.1f;

    int kernalIndexSlime;
    int kernalIndexProcess;
    private Agent[] agents;

    private void Awake() {
        TrailMap = new RenderTexture(width, height, 24);
        TrailMap.enableRandomWrite = true;
        TrailMap.Create();

        kernalIndexSlime = slimeShader.FindKernel("RunSlime");
        kernalIndexProcess = slimeShader.FindKernel("ProcessTrailMap");
        CreateAgents();

        //slimeShader.SetTexture(kernalIndexSlime, "Result", tex);
        //slimeShader.Dispatch(kernalIndexSlime, width / 8, height / 8, 1);

        // we now should have a texture for the screen
    }
    private void Update() {
        RunSlimeShader();
        ProcessTrailMap();
    }

    void CreateAgents() {
        agents = new Agent[numAgents];
        // randomly make the agents, should this be a ComputeShader??
        for (int i = 0; i < numAgents; i++) {
            Vector2 pos = new Vector2(width / 2.0f, height / 2.0f) + Random.insideUnitCircle * spawnRadius;
            float angle = Random.Range(0.0f, 360.0f);
            agents[i] = new Agent {
                position = pos,
                angle = angle
            };
        }
    }

    void ProcessTrailMap() {
        slimeShader.SetTexture(kernalIndexProcess, "TrailMap", TrailMap);
        slimeShader.SetFloat("decayRate", decayRate);

        slimeShader.Dispatch(kernalIndexProcess, width / 8, height / 8, 1);
    }

    void RunSlimeShader() {
        // change tex here
        slimeShader.SetInt("width", width);
        slimeShader.SetInt("height", height);
        slimeShader.SetInt("numAgents", numAgents);
        slimeShader.SetFloat("moveSpeed", moveSpeed);
        slimeShader.SetFloat("deltaTime", Time.deltaTime);
        slimeShader.SetFloat("PI", Mathf.PI);

        slimeShader.SetTexture(kernalIndexSlime, "TrailMap", TrailMap);

        ComputeBuffer buffer = new ComputeBuffer(numAgents, 3 * sizeof(float)); // stride is size of each element, in this case 3 floats (2 for vector, 1 for angle) by size of float
        buffer.SetData(agents); // set the data
        slimeShader.SetBuffer(kernalIndexSlime, "agents", buffer);

        slimeShader.Dispatch(kernalIndexSlime, numAgents, 1, 1);

        buffer.GetData(agents);
        buffer.Release();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(TrailMap, destination);
    }
}
