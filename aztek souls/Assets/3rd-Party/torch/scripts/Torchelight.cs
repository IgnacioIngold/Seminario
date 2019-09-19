using UnityEngine;
using System.Collections;

public class Torchelight : MonoBehaviour {
	
	public Light TorchLight;
	public ParticleSystem MainFlame;
	public ParticleSystem BaseFlame;
	public ParticleSystem Etincelles;
	public ParticleSystem Fumee;
	public float MaxLightIntensity;
	public float IntensityLight;

    //ParticleSystem.EmissionModule flameEmission;
    //ParticleSystem.EmissionModule baseFlameEmission;
    //ParticleSystem.EmissionModule etincellesEmission;
    //ParticleSystem.EmissionModule FumeeEmission;

    private void Awake()
    {
        TorchLight.intensity = IntensityLight;
        var flameEmission = MainFlame.emission;
        flameEmission.rateOverTime = IntensityLight * 20f;

        var baseFlameEmission = BaseFlame.emission;
        baseFlameEmission.rateOverTime = IntensityLight * 15f;

        var etincellesEmission = Etincelles.emission;
        etincellesEmission.rateOverTime = IntensityLight * 7f;

        var FumeeEmission = Fumee.emission;
        FumeeEmission.rateOverTime = IntensityLight * 12f;
    }
	

	//void Update () {
	//	if (IntensityLight<0) IntensityLight=0;
	//	if (IntensityLight>MaxLightIntensity) IntensityLight=MaxLightIntensity;		

	//	TorchLight.intensity = IntensityLight/2f+Mathf.Lerp(IntensityLight-0.1f,IntensityLight+0.1f,Mathf.Cos(Time.time*30));

	//	//TorchLight.GetComponent<Light>().color=new Color(Mathf.Min(IntensityLight/1.5f,1f),Mathf.Min(IntensityLight/2f,1f),0f);
	//	MainFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*20f;
	//	BaseFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*15f;
	//	Etincelles.GetComponent<ParticleSystem>().emissionRate=IntensityLight*7f;
	//	Fumee.GetComponent<ParticleSystem>().emissionRate=IntensityLight*12f;		

	//}
}
