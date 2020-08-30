using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLeft : Bullet
{
    // this bullet goes to the left
    public override void SetDirection() {
        horizontal = -1;
    }
}
