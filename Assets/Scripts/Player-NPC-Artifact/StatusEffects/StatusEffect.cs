using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    protected int duration;
    protected EntityStats owner;
    protected string id;

    public StatusEffect(int duration) {
        this.duration = duration;
	}

    public void OnTurnStart() {
        --duration;
        if (duration <= 0) owner.QueueStatusEffectForRemoval(this);
	}

    public virtual void OnApply(EntityStats owner) {
        this.owner = owner;
	}
    public abstract void OnRemove();

    public string ID { get => id; }
    public int Duration { get => duration; set => duration = value; }
    public EntityStats Owner { get => owner;}
}
