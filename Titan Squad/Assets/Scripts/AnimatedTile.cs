﻿using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps
{
    /// <summary>
    /// Animated Tiles are tiles which run through and display a list of sprites in sequence.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Animated Tile", menuName = "Tiles/Animated Tile")]
    public class AnimatedTile : TileBase
    {
        /// <summary>
        /// The List of Sprites set for the Animated Tile.
        /// This will be played in sequence.
        /// </summary>
        public Sprite[] m_AnimatedSprites;
        /// <summary>
        /// The minimum possible speed at which the Animation of the Tile will be played.
        /// A speed value will be randomly chosen between the minimum and maximum speed.
        /// </summary>
        public float m_AnimSpeed = .5f;
        /// <summary>
        /// The maximum possible speed at which the Animation of the Tile will be played.
        /// A speed value will be randomly chosen between the minimum and maximum speed.
        /// </summary>
        
        public float m_AnimationStartTime;
        /// <summary>
        /// The Collider Shape generated by the Tile.
        /// </summary>
        public Tile.ColliderType m_TileColliderType;

        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            tileData.transform = Matrix4x4.identity;
            tileData.color = Color.white;
            if (m_AnimatedSprites != null && m_AnimatedSprites.Length > 0)
            {
                tileData.sprite = m_AnimatedSprites[m_AnimatedSprites.Length - 1];
                tileData.colliderType = m_TileColliderType;
            }
        }

        /// <summary>
        /// Retrieves any tile animation data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileAnimationData">Data to run an animation on the tile.</param>
        /// <returns>Whether the call was successful.</returns>
        public override bool GetTileAnimationData(Vector3Int location, ITilemap tileMap, ref TileAnimationData tileAnimationData)
        {
            if (m_AnimatedSprites.Length > 0)
            {
                tileAnimationData.animatedSprites = m_AnimatedSprites;
                tileAnimationData.animationSpeed = m_AnimSpeed;
                tileAnimationData.animationStartTime = m_AnimationStartTime;
                return true;
            }
            return false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AnimatedTile))]
    public class AnimatedTileEditor : Editor
    {
        private AnimatedTile tile { get { return (target as AnimatedTile); } }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            

            EditorGUILayout.LabelField("Drag sprites onto the list below.");
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AnimatedSprites"), true);
            serializedObject.ApplyModifiedProperties();

            int count = 0;
            if (tile.m_AnimatedSprites != null)
                count = tile.m_AnimatedSprites.Length;

            if (count == 0)
                return;
            

            float AnimationSpeed = EditorGUILayout.FloatField("Animation Speed", tile.m_AnimSpeed);
            if (AnimationSpeed < 0.0f)
                AnimationSpeed = 0.0f;
            

            tile.m_AnimSpeed = AnimationSpeed;

            tile.m_AnimationStartTime = EditorGUILayout.FloatField("Start Time", tile.m_AnimationStartTime);
            tile.m_TileColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Collider Type", tile.m_TileColliderType);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(tile);
        }
    }
#endif
}