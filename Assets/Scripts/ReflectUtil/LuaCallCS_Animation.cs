using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LuaInterface;
using System.Collections;

public static partial class LuaCallCS
{
    public static Tweener PlayPositionAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startPos = default, Vector3 endPos = default, float duration = 0, LuaFunction luaFunc = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localPosition = startPos;

                tweener = trans.DOLocalMove(endPos, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                trans.localPosition = endPos;
            }
        }

        return tweener;
    }

    public static Tweener PlayRotationAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startRot = default, Vector3 endRot = default, float duration = 0, LuaFunction luaFunc = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Incremental, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localRotation = Quaternion.Euler(startRot);

                tweener = trans.DOLocalRotate(endRot, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                trans.localRotation = Quaternion.Euler(endRot);
            }
        }

        return tweener;
    }

    public static Tweener PlayScaleAnimation(UnityEngine.Object obj, string childPath = "", Vector3 startScale = default, Vector3 endScale = default, float duration = 0, LuaFunction luaFunc = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            if (duration > 0)
            {
                trans.localScale = startScale;

                tweener = trans.DOScale(endScale, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                trans.localScale = endScale;
            }
        }

        return tweener;
    }

    public static Tweener PlayAlphaAnimation(UnityEngine.Object obj, string childPath = "", float startAlpha = 0, float endAlpha = 0, float duration = 0, LuaFunction luaFunc = null, float delay = 0, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        CanvasGroup canvasGroup = trans.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            if (duration > 0)
            {
                canvasGroup.alpha = startAlpha;

                tweener = canvasGroup.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                canvasGroup.alpha = endAlpha;
            }

            return tweener;
        }

        Image image = trans.GetComponent<Image>();

        if (image != null)
        {
            if (duration > 0)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, startAlpha);

                tweener = image.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            }

            return tweener;
        }

        Text text = trans.GetComponent<Text>();

        if (text != null)
        {
            if (duration > 0)
            {
                text.color = new Color(image.color.r, image.color.g, image.color.b, startAlpha);

                tweener = text.DOFade(endAlpha, duration);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                text.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            }

            return tweener;
        }

        return tweener;
    }

    public static Tweener PlayCurveAnimation(UnityEngine.Object obj, string childPath = "", LuaTable posTable = null, float duration = 0, LuaFunction luaFunc = null, float delay = 0, PathType pathType = PathType.CatmullRom, int loopCount = 0, LoopType loopType = LoopType.Yoyo, Ease moveWay = Ease.Linear)
    {
        Tweener tweener = null;

        if (posTable == null)
        {
            return tweener;
        }

        Transform trans = GetTransform(obj);

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans != null)
        {
            Vector3[] posArray = new Vector3[posTable.Length];

            for (int i = 0; i < posTable.Length; i++)
            {
                posArray[i] = (Vector3)posTable[i + 1];
            }

            if (duration > 0)
            {
                trans.localPosition = posArray[0];

                PathMode pathMode = PathMode.TopDown2D;

                foreach (var item in posArray)
                {
                    if (item.z != 0)
                    {
                        pathMode = PathMode.Full3D;
                        break;
                    }
                }

                tweener = trans.DOLocalPath(posArray, duration, pathType, pathMode, 100);

                tweener.SetEase(moveWay);

                if (loopCount != 0)
                {
                    tweener.SetLoops(loopCount, loopType);//次数为-1则无限循环
                }

                if (delay > 0)
                {
                    tweener.SetDelay(delay);
                }

                if (luaFunc != null)
                {
                    tweener.OnComplete(luaFunc.Call);
                }
            }
            else
            {
                trans.localPosition = posArray[posArray.Length - 1];
            }
        }

        return tweener;
    }

    public static void ShowAnimationByTime(UnityEngine.Object obj, string childPath = "", string animName = "", float time = 0)
    {
        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        Transform trans = GetTransform(obj);

        if (trans == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans == null)
        {
            return;
        }

        Animation animation = trans.GetComponent<Animation>();

        if (animation != null && animation.Play(animName))
        {
            AnimationState animationState = animation[animName];
            animationState.time = time;
            animation.Sample();
            animation.Stop();
        }
    }

    public static void PlayAnimation(UnityEngine.Object obj, string childPath = "", string animName = "", WrapMode wrapMode = WrapMode.Once, LuaFunction callBack = null)
    {
        CoroutineManager.Instance.InvokePlayAnimation(obj, childPath, animName, wrapMode, callBack);
    }

    public static void StopAnimation(UnityEngine.Object obj, string childPath = "", string animName = "")
    {
        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        Transform trans = GetTransform(obj);

        if (trans == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(childPath))
        {
            trans = trans.Find(childPath);
        }

        if (trans == null)
        {
            return;
        }

        Animation animation = trans.GetComponent<Animation>();

        if (animation != null)
        {
            animation.Stop(animName);
        }
    }
}