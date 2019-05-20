/************************************************************************************
 Copyright: Copyright 2014 Beijing Noitom Technology Ltd. All Rights reserved.
 Pending Patents: PCT/CN2014/085659 PCT/CN2014/071006

 Licensed under the Perception Neuron SDK License Beta Version (the “License");
 You may only use the Perception Neuron SDK when in compliance with the License,
 which is provided at the time of installation or download, or which
 otherwise accompanies this software in the form of either an electronic or a hard copy.

 A copy of the License is included with this package or can be obtained at:
 http://www.neuronmocap.com

 Unless required by applicable law or agreed to in writing, the Perception Neuron SDK
 distributed under the License is provided on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing conditions and
 limitations under the License.
************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Neuron;
using System.IO;

public class NeuronAnimatorInstance : NeuronInstance
{
    public static class Eklem_Degerleri
    {
        public static double Sol_Bacak = -90;
        public static double Sag_Bacak = -90;
        public static double Sol_Diz = 0.0;
        public static double Sag_Diz = 0.0;

        public static double Sol_Omuz = 0.0;
        public static double Sag_Omuz = 0.0;
        public static double Sol_Dirsek = 0.0;
        public static double Sag_Dirsek = 0.0;

        public static double Durus = 110.0;
    }

    public GameObject Deger_Gosterici;
    private Animator Animator;


    public Animator boundAnimator = null;
    public bool physicalUpdate = false;
	public bool enableHipsMovement = false;

    public NeuronAnimatorPhysicalReference physicalReference = new NeuronAnimatorPhysicalReference();
    Vector3[] bonePositionOffsets = new Vector3[(int)HumanBodyBones.LastBone];
    Vector3[] boneRotationOffsets = new Vector3[(int)HumanBodyBones.LastBone];

    public static float velocityMagic = 3000.0f;
    public static float angularVelocityMagic = 70.0f;

    public NeuronAnimatorInstance()
    {
    }

    public NeuronAnimatorInstance(string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID)
        : base(address, port, commandServerPort, socketType, actorID)
    {
    }

    public NeuronAnimatorInstance(Animator animator, string address, int port, int commandServerPort, NeuronConnection.SocketType socketType, int actorID)
        : base(address, port, commandServerPort, socketType, actorID)
    {
        boundAnimator = animator;
        UpdateOffset();
    }

    public NeuronAnimatorInstance(Animator animator, NeuronActor actor)
        : base(actor)
    {
        boundAnimator = animator;
        UpdateOffset();
    }

    public NeuronAnimatorInstance(NeuronActor actor)
        : base(actor)
    {
    }

    new void OnEnable()
    {
        base.OnEnable();
        if (boundAnimator == null)
        {
            boundAnimator = GetComponent<Animator>();
            UpdateOffset();
        }

        Animator = boundAnimator;

    }

    new void Update()
    {
        base.ToggleConnect();
        base.Update();

        if (boundActor != null && boundAnimator != null && !physicalUpdate)
        {
            if (physicalReference.Initiated())
            {
                ReleasePhysicalContext();
            }

			ApplyMotion(boundActor, boundAnimator, bonePositionOffsets, boneRotationOffsets, enableHipsMovement, hipHeightOffset);
        }
    }

    void FixedUpdate()
    {
        base.ToggleConnect();

        if (boundActor != null && boundAnimator != null && physicalUpdate)
        {
            if (!physicalReference.Initiated())
            {
                physicalUpdate = InitPhysicalContext();
            }

            ApplyMotionPhysically(physicalReference.GetReferenceAnimator(), boundAnimator);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.Euler(new Vector3(0, 110, 0)));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(new Vector3(-90, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(new Vector3(-90, 0, 0)));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sol_Diz, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sag_Diz, 0, 0)));
    }

    void LateUpdate()
    {
        if (Deger_Gosterici != null)
        {
            Deger_Gosterici.GetComponent<TextMesh>().text = "";
            Deger_Gosterici.GetComponent<TextMesh>().text += "Diz: " + Deger_Duzenle(Eklem_Degerleri.Sag_Diz).ToString("000");
        }
    }

    static bool ValidateVector3(Vector3 vec)
    {
        return !float.IsNaN(vec.x) && !float.IsNaN(vec.y) && !float.IsNaN(vec.z)
            && !float.IsInfinity(vec.x) && !float.IsInfinity(vec.y) && !float.IsInfinity(vec.z);
    }

    static void SetScale(Animator animator, HumanBodyBones bone, float size, float referenceSize)
    {
        Transform t = animator.GetBoneTransform(bone);
        if (t != null && bone <= HumanBodyBones.Jaw)
        {
            float ratio = size / referenceSize;

            Vector3 newScale = new Vector3(ratio, ratio, ratio);
            newScale.Scale(new Vector3(1.0f / t.parent.lossyScale.x, 1.0f / t.parent.lossyScale.y, 1.0f / t.parent.lossyScale.z));

            if (ValidateVector3(newScale))
            {
                t.localScale = newScale;
            }
        }
    }

    // set position for bone in animator
    static void SetPosition(Animator animator, HumanBodyBones bone, Vector3 pos)
    {
        Transform t = animator.GetBoneTransform(bone);
        if (t != null)
        {
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                t.localPosition = pos;
            }
        }
    }

    // set rotation for bone in animator
    static void SetRotation(Animator animator, HumanBodyBones bone, Vector3 rotation)
    {


        Transform t = animator.GetBoneTransform(bone);
        if (t != null)
        {
            Quaternion rot = Quaternion.Euler(rotation);
            if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
            {
                t.localRotation = rot;
            }
        }
    }

    // apply transforms extracted from actor mocap data to transforms of animator bones
	public static void ApplyMotion(NeuronActor actor, Animator animator, Vector3[] positionOffsets, Vector3[] rotationOffsets, bool hipsMove, float hipOffset)
    {
        if (actor == null || actor.timeStamp == 0)
        {
            return;
        }

        //Eklem_Degerleri.Sol_Diz = actor.GetReceivedRotation(NeuronBones.LeftLeg)[0];
        Eklem_Degerleri.Sag_Diz = actor.GetReceivedRotation(NeuronBones.RightLeg)[0];



        // apply Hips position
        //if (hipsMove) {
        //	Vector3 hipPos = actor.GetReceivedPosition (NeuronBones.Hips);
        //	SetPosition (animator, HumanBodyBones.Hips, new Vector3(hipPos.x, hipPos.y + hipOffset, hipPos.z) ); // + positionOffsets [(int)HumanBodyBones.Hips]);
        //	SetRotation (animator, HumanBodyBones.Hips, actor.GetReceivedRotation (NeuronBones.Hips));
        //}


        // apply rotations

        //// legs
        //SetRotation(animator, HumanBodyBones.RightUpperLeg, actor.GetReceivedRotation(NeuronBones.RightUpLeg));
        //SetRotation(animator, HumanBodyBones.RightLowerLeg, actor.GetReceivedRotation(NeuronBones.RightLeg));
        //SetRotation(animator, HumanBodyBones.RightFoot, actor.GetReceivedRotation(NeuronBones.RightFoot));
        //SetRotation(animator, HumanBodyBones.LeftUpperLeg, actor.GetReceivedRotation(NeuronBones.LeftUpLeg));
        //SetRotation(animator, HumanBodyBones.LeftLowerLeg, actor.GetReceivedRotation(NeuronBones.LeftLeg));
        //SetRotation(animator, HumanBodyBones.LeftFoot, actor.GetReceivedRotation(NeuronBones.LeftFoot));

        //// spine
        //SetRotation(animator, HumanBodyBones.Spine, actor.GetReceivedRotation(NeuronBones.Spine));
        //SetRotation(animator, HumanBodyBones.Chest, actor.GetReceivedRotation(NeuronBones.Spine1) + actor.GetReceivedRotation(NeuronBones.Spine2) + actor.GetReceivedRotation(NeuronBones.Spine3));
        //SetRotation(animator, HumanBodyBones.Neck, actor.GetReceivedRotation(NeuronBones.Neck));
        //SetRotation(animator, HumanBodyBones.Head, actor.GetReceivedRotation(NeuronBones.Head));

        //// right arm
        //SetRotation(animator, HumanBodyBones.RightShoulder, actor.GetReceivedRotation(NeuronBones.RightShoulder));
        //SetRotation(animator, HumanBodyBones.RightUpperArm, actor.GetReceivedRotation(NeuronBones.RightArm));
        //SetRotation(animator, HumanBodyBones.RightLowerArm, actor.GetReceivedRotation(NeuronBones.RightForeArm));

        //// right hand
        //SetRotation(animator, HumanBodyBones.RightHand, actor.GetReceivedRotation(NeuronBones.RightHand));
        //SetRotation(animator, HumanBodyBones.RightThumbProximal, actor.GetReceivedRotation(NeuronBones.RightHandThumb1));
        //SetRotation(animator, HumanBodyBones.RightThumbIntermediate, actor.GetReceivedRotation(NeuronBones.RightHandThumb2));
        //SetRotation(animator, HumanBodyBones.RightThumbDistal, actor.GetReceivedRotation(NeuronBones.RightHandThumb3));

        //SetRotation(animator, HumanBodyBones.RightIndexProximal, actor.GetReceivedRotation(NeuronBones.RightHandIndex1) + actor.GetReceivedRotation(NeuronBones.RightInHandIndex));
        //SetRotation(animator, HumanBodyBones.RightIndexIntermediate, actor.GetReceivedRotation(NeuronBones.RightHandIndex2));
        //SetRotation(animator, HumanBodyBones.RightIndexDistal, actor.GetReceivedRotation(NeuronBones.RightHandIndex3));

        //SetRotation(animator, HumanBodyBones.RightMiddleProximal, actor.GetReceivedRotation(NeuronBones.RightHandMiddle1) + actor.GetReceivedRotation(NeuronBones.RightInHandMiddle));
        //SetRotation(animator, HumanBodyBones.RightMiddleIntermediate, actor.GetReceivedRotation(NeuronBones.RightHandMiddle2));
        //SetRotation(animator, HumanBodyBones.RightMiddleDistal, actor.GetReceivedRotation(NeuronBones.RightHandMiddle3));

        //SetRotation(animator, HumanBodyBones.RightRingProximal, actor.GetReceivedRotation(NeuronBones.RightHandRing1) + actor.GetReceivedRotation(NeuronBones.RightInHandRing));
        //SetRotation(animator, HumanBodyBones.RightRingIntermediate, actor.GetReceivedRotation(NeuronBones.RightHandRing2));
        //SetRotation(animator, HumanBodyBones.RightRingDistal, actor.GetReceivedRotation(NeuronBones.RightHandRing3));

        //SetRotation(animator, HumanBodyBones.RightLittleProximal, actor.GetReceivedRotation(NeuronBones.RightHandPinky1) + actor.GetReceivedRotation(NeuronBones.RightInHandPinky));
        //SetRotation(animator, HumanBodyBones.RightLittleIntermediate, actor.GetReceivedRotation(NeuronBones.RightHandPinky2));
        //SetRotation(animator, HumanBodyBones.RightLittleDistal, actor.GetReceivedRotation(NeuronBones.RightHandPinky3));

        //// left arm
        //SetRotation(animator, HumanBodyBones.LeftShoulder, actor.GetReceivedRotation(NeuronBones.LeftShoulder));
        //SetRotation(animator, HumanBodyBones.LeftUpperArm, actor.GetReceivedRotation(NeuronBones.LeftArm));
        //SetRotation(animator, HumanBodyBones.LeftLowerArm, actor.GetReceivedRotation(NeuronBones.LeftForeArm));

        //// left hand
        //SetRotation(animator, HumanBodyBones.LeftHand, actor.GetReceivedRotation(NeuronBones.LeftHand));
        //SetRotation(animator, HumanBodyBones.LeftThumbProximal, actor.GetReceivedRotation(NeuronBones.LeftHandThumb1));
        //SetRotation(animator, HumanBodyBones.LeftThumbIntermediate, actor.GetReceivedRotation(NeuronBones.LeftHandThumb2));
        //SetRotation(animator, HumanBodyBones.LeftThumbDistal, actor.GetReceivedRotation(NeuronBones.LeftHandThumb3));

        //SetRotation(animator, HumanBodyBones.LeftIndexProximal, actor.GetReceivedRotation(NeuronBones.LeftHandIndex1) + actor.GetReceivedRotation(NeuronBones.LeftInHandIndex));
        //SetRotation(animator, HumanBodyBones.LeftIndexIntermediate, actor.GetReceivedRotation(NeuronBones.LeftHandIndex2));
        //SetRotation(animator, HumanBodyBones.LeftIndexDistal, actor.GetReceivedRotation(NeuronBones.LeftHandIndex3));

        //SetRotation(animator, HumanBodyBones.LeftMiddleProximal, actor.GetReceivedRotation(NeuronBones.LeftHandMiddle1) + actor.GetReceivedRotation(NeuronBones.LeftInHandMiddle));
        //SetRotation(animator, HumanBodyBones.LeftMiddleIntermediate, actor.GetReceivedRotation(NeuronBones.LeftHandMiddle2));
        //SetRotation(animator, HumanBodyBones.LeftMiddleDistal, actor.GetReceivedRotation(NeuronBones.LeftHandMiddle3));

        //SetRotation(animator, HumanBodyBones.LeftRingProximal, actor.GetReceivedRotation(NeuronBones.LeftHandRing1) + actor.GetReceivedRotation(NeuronBones.LeftInHandRing));
        //SetRotation(animator, HumanBodyBones.LeftRingIntermediate, actor.GetReceivedRotation(NeuronBones.LeftHandRing2));
        //SetRotation(animator, HumanBodyBones.LeftRingDistal, actor.GetReceivedRotation(NeuronBones.LeftHandRing3));

        //SetRotation(animator, HumanBodyBones.LeftLittleProximal, actor.GetReceivedRotation(NeuronBones.LeftHandPinky1) + actor.GetReceivedRotation(NeuronBones.LeftInHandPinky));
        //SetRotation(animator, HumanBodyBones.LeftLittleIntermediate, actor.GetReceivedRotation(NeuronBones.LeftHandPinky2));
        //SetRotation(animator, HumanBodyBones.LeftLittleDistal, actor.GetReceivedRotation(NeuronBones.LeftHandPinky3));
    }

    // apply Transforms of src bones to Rigidbody Components of dest bones
    public static void ApplyMotionPhysically(Animator src, Animator dest)
    {
        for (HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i)
        {
            Transform src_transform = src.GetBoneTransform(i);
            Transform dest_transform = dest.GetBoneTransform(i);
            if (src_transform != null && dest_transform != null)
            {
                Rigidbody rigidbody = dest_transform.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    Quaternion dAng = src_transform.rotation * Quaternion.Inverse(dest_transform.rotation);
                    float angle = 0.0f;
                    Vector3 axis = Vector3.zero;
                    dAng.ToAngleAxis(out angle, out axis);
                    Vector3 velocityTarget = (src_transform.position - dest_transform.position) * velocityMagic * Time.fixedDeltaTime;
                    Vector3 angularTarget = (Time.fixedDeltaTime * angle * axis) * angularVelocityMagic;

                    ApplyVelocity(rigidbody, velocityTarget, angularTarget);

                    //rigidbody.MovePosition(src_transform.position);
                    //rigidbody.MoveRotation(src_transform.rotation);
                }
            }
        }
    }

    static void ApplyVelocity(Rigidbody rb, Vector3 velocityTarget, Vector3 angularTarget)
    {
        Vector3 v = Vector3.MoveTowards(rb.velocity, velocityTarget, 10.0f);
        if( ValidateVector3( v ) )
        {
            rb.velocity = v;
        }

        v = Vector3.MoveTowards(rb.angularVelocity, angularTarget, 10.0f);
        if( ValidateVector3( v ) )
        {
            rb.angularVelocity = v;
        }

		Debug.Log ("Apply angular velocity to rb: " + rb.name + " velocityTarget: " + velocityTarget + " angularTarget: " + angularTarget + " new angular velocity: " + v);
    }

    bool InitPhysicalContext()
    {
        if (physicalReference.Init(boundAnimator))
        {
            // break original object's hierachy of transforms, so we can use MovePosition() and MoveRotation() to set transform
            NeuronHelper.BreakHierarchy(boundAnimator);
            return true;
        }

        return false;
    }

    void ReleasePhysicalContext()
    {
        physicalReference.Release();
    }

    void UpdateOffset()
    {
        // we do some adjustment for the bones here which would replaced by our model retargeting later

        // initiate values
        for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
        {
            bonePositionOffsets[i] = Vector3.zero;
            boneRotationOffsets[i] = Vector3.zero;
        }

        //if (boundAnimator != null)
        //{
        //    Transform leftLegTransform = boundAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        //    Transform rightLegTransform = boundAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        //    if (leftLegTransform != null)
        //    {
        //        bonePositionOffsets[(int)HumanBodyBones.LeftUpperLeg] = new Vector3(0.0f, leftLegTransform.localPosition.y, 0.0f);
        //        bonePositionOffsets[(int)HumanBodyBones.RightUpperLeg] = new Vector3(0.0f, rightLegTransform.localPosition.y, 0.0f);
        //        bonePositionOffsets[(int)HumanBodyBones.Hips] = new Vector3(0.0f, -(leftLegTransform.localPosition.y + rightLegTransform.localPosition.y) * 0.5f, 0.0f);
        //    }
        //}
    }

    double Deger_Duzenle(double Aci)
    {
        if (Aci > 180)
        {
            Aci = -1 * (360 - Aci);
        }

        return Aci;
    }
}