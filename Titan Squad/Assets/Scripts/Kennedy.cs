using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kennedy : PlayerUnit
{
    const int ADVANCED_SCOPE_DURATION = 1;
    const int DECOY_DURATION = 1;

    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        abilityNames[0] = "Advanced Scope";
        abilityNames[1] = "Instantaneous Transmission";
        abilityNames[2] = "Decoy";

        abilityDescriptions[0] = "Kennedy triggers her advanced scope, marking all enemies within range of her Railgun for " + ADVANCED_SCOPE_DURATION + " turn.";
        abilityDescriptions[1] = "Kennedy places either a deposit spot or a teleport trap. Anyone who steps onto a teleport trap is instantaneously transmitted to the deposit spot and their movement is ended. Maximum of 1 deposit spot and teleport trap.";
        abilityDescriptions[2] = "Kennedy activates her personal decoy, turning herself invisible for " + DECOY_DURATION + " turn and sending a controllable decoy out while invisible.";

    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Advanced Scope
    override
    public void ability1()
    {

    }

    //Instantaneous Transmission
    override
    public void ability2()
    {

    }

    //Decoy
    override
    public void ability3()
    {

    }
}
