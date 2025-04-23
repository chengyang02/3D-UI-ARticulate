using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.EventSystems;

namespace FormatGames
{
    public static class UIUtilities
    {
        public static void HideUIElement(RectTransform target, bool value, Vector2 defaultLocalPosition)
        {
            if(value)
            {
                target.anchoredPosition = new Vector2(9999, 9999);
            }
            else
            {
                target.localPosition = defaultLocalPosition;
            }
           
        }
        public static void OrbitTarget(RectTransform target, Vector2 direction, float distance, bool rotateTarget = true)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180;
            Vector3 offset = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) * distance;

            target.localPosition = offset;
            target.localRotation = rotateTarget ? Quaternion.Euler(0, 0, angle) : target.localRotation;
        }

        public static Vector2 FitGreatestChild(RectTransform target, List<RectTransform> ignoreObjects = null)
        {
            if (target.childCount == 0) return target.sizeDelta;

            RectTransform chosenOne = null;
            RectTransform farthestElement = null;
            RectTransform biggestElement = null;
            float greatestDistance = 0f;
            float greatestSize = 0f;

            // Encuentra el objeto más lejano y el más grande en un solo bucle
            foreach (RectTransform rect in target)
            {
                if (!rect.gameObject.activeSelf || (ignoreObjects != null && ignoreObjects.Contains(rect)))
                    continue;

                float recDistance = rect.localPosition.magnitude;
                float sizeMagnitude = rect.sizeDelta.magnitude;

                if (recDistance + sizeMagnitude / 2 >= greatestDistance)
                {
                    greatestDistance = recDistance + sizeMagnitude / 2;
                    farthestElement = rect;
                }

                if (sizeMagnitude >= greatestSize)
                {
                    greatestSize = sizeMagnitude;
                    biggestElement = rect;
                }
            }

            if (farthestElement == null || biggestElement == null) return target.sizeDelta;

            // Obtener esquinas del más lejano en el espacio local del más grande
            Vector3[] cornersA = new Vector3[4];
            farthestElement.GetWorldCorners(cornersA);

            for (int i = 0; i < 4; i++)
                cornersA[i] = biggestElement.InverseTransformPoint(cornersA[i]);

            // Obtener los límites de biggestElement
            Vector3[] cornersB = new Vector3[4];
            biggestElement.GetWorldCorners(cornersB);
            float minX = biggestElement.InverseTransformPoint(cornersB[0]).x;
            float maxX = biggestElement.InverseTransformPoint(cornersB[2]).x;
            float minY = biggestElement.InverseTransformPoint(cornersB[0]).y;
            float maxY = biggestElement.InverseTransformPoint(cornersB[2]).y;

            // Verificar si todas las esquinas del más lejano están dentro de los límites del más grande
            bool isInside = true;
            foreach (var corner in cornersA)
            {
                if (corner.x < minX || corner.x > maxX || corner.y < minY || corner.y > maxY)
                {
                    isInside = false;
                    break;
                }
            }

            // Elegir el objeto correcto
            chosenOne = isInside ? biggestElement : farthestElement;

            if (chosenOne == null) return target.sizeDelta;

            // Obtiene las esquinas del objeto más lejano
            Vector3[] corners = new Vector3[4];
            chosenOne.GetWorldCorners(corners);

            // Convierte las esquinas al espacio local del target
            Vector3 min = target.InverseTransformPoint(corners[0]);
            Vector3 max = target.InverseTransformPoint(corners[2]);

            // Calcula el nuevo tamaño cuadrado basado en la mayor diferencia
            float newSize = Mathf.Max(Mathf.Abs(min.x), Mathf.Abs(max.x), Mathf.Abs(min.y), Mathf.Abs(max.y)) * 2f;

            return new Vector2(newSize, newSize);
        }

        public static void ResizeToFitChildren(RectTransform target)
        {
            Vector3 originalPos = target.position;
            if (target.childCount == 0) return;

            
            // Quitar los hijos temporalmente
            Transform[] children = new Transform[target.childCount];

            for (int i = 0; i < target.childCount; i++)
            {
                children[i] = target.GetChild(i);
            }

            foreach (Transform child in children)
            {
                child.SetParent(target.parent, true);
            }

            // Forzar actualización del layout
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);

            target.localPosition = Vector3.zero;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (Transform child in children)
            {
                if (child == null || !child.gameObject.activeSelf) continue;

                Vector3[] corners = new Vector3[4];
                RectTransform rectChild = child.GetComponent<RectTransform>();

                rectChild.GetWorldCorners(corners);

                for (int i = 0; i < 4; i++)
                {
                    Vector3 localPoint = target.InverseTransformPoint(corners[i]);
                    min = Vector2.Min(min, localPoint);
                    max = Vector2.Max(max, localPoint);
                }
            }

            target.offsetMin = min;
            target.offsetMax = max;

            target.position = originalPos;

            // Volver a asignar los hijos
            foreach (Transform child in children)
            {
                child.SetParent(target, true);
            }
        }

        // ------------------------------------------------------------- SPRITE -----------------------------------------------------------------//

#if UNITY_EDITOR

        public static Sprite GetSpriteOnAssets(string name)
        {
            string path;

            // Busca activos que sean de tipo Texture2D o PSD (t:Texture2D ya cubre PSD también)
            string[] guids = AssetDatabase.FindAssets(name + " t:Texture2D");
            if (guids.Length == 0) return null; // No se encontró el archivo

            path = AssetDatabase.GUIDToAssetPath(guids[0]); // Obtiene la ruta del archivo

            // Carga todos los assets en la ruta (incluyendo sprites de imágenes PSD y con Sprite Mode: Multiple)
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // Encuentra el primer sprite válido dentro del archivo
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    return sprite; // Devuelve el primer sprite encontrado
                }
            }

            return null; // No se encontraron sprites
        }
        public static List<Sprite> GetSpritesFromAssets(string textureName, List<string> spriteNames)
        {
            List<Sprite> spritesFound = new List<Sprite>();

            // Busca la textura (puede ser PNG, PSD, etc.)
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");
            if (guids.Length == 0) return spritesFound; // No se encontró la textura

            string path = AssetDatabase.GUIDToAssetPath(guids[0]); // Obtiene la ruta del archivo

            // Carga todos los assets en la ruta (incluyendo sprites de PSD o Sprite Mode: Multiple)
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // Filtra y agrega solo los sprites con los nombres especificados
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && spriteNames.Contains(sprite.name))
                {
                    spritesFound.Add(sprite);
                }
            }

            return spritesFound; // Devuelve la lista de sprites encontrados
        }

        public static Sprite GetSpritesFromAssets(string textureName, string spriteNames)
        {
            Sprite spritesFound = null;

            // Busca la textura (puede ser PNG, PSD, etc.)
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");
            if (guids.Length == 0) return spritesFound; // No se encontró la textura

            string path = AssetDatabase.GUIDToAssetPath(guids[0]); // Obtiene la ruta del archivo

            // Carga todos los assets en la ruta (incluyendo sprites de PSD o Sprite Mode: Multiple)
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // Filtra y agrega solo los sprites con los nombres especificados
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && spriteNames.Contains(sprite.name))
                {
                    spritesFound = sprite;
                }
            }

            return spritesFound; // Devuelve la lista de sprites encontrados
        }

        public static Sprite GetSpriteOnAsset(Sprite target)
        {
            if (target == null) return null;

#if UNITY_EDITOR
            string spriteName = target.name;
            string textureName = target.texture.name; // Obtener el nombre de la textura del sprite

            Sprite spriteFound = null;

            // Buscar la textura en el proyecto
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No se encontró ninguna textura para: {textureName}");
                return null;
            }

            // Buscar en todas las rutas encontradas
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                // Buscar en los sprites de la textura
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite && sprite.name == spriteName)
                    {
                        spriteFound = sprite;
                        break;
                    }
                }

                if (spriteFound != null) break; // Detener la búsqueda si ya lo encontró
            }

            if (spriteFound == null)
            {
                Debug.LogWarning($"No se encontró el sprite '{spriteName}' en la textura '{textureName}'.");
            }

            return spriteFound;
#else
        return null;
#endif
        }

#endif
    }
}

