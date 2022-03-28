using UnityEngine;
using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using Scopa;
using System.IO;

namespace Scopa.Editor {

    /// <summary>
    /// custom Unity importer that detects .WAD files in /Assets/ and automatically imports textures (and generates materials)
    /// </summary>
    [ScriptedImporter(1, "wad")]
    public class WadImporter : ScriptedImporter
    {
        /// <summary> internal default config settings that every WAD Importer generates for itself</summary>
        public ScopaWadConfig config;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var filepath = Application.dataPath + ctx.assetPath.Substring("Assets".Length);
            
            if ( config == null ) {
                config = new ScopaWadConfig();
            }

            var wad = ScopaCore.ParseWad(filepath);
            var textures = ScopaCore.BuildWadTextures(wad, config);

            foreach (var tex in textures) {
                ctx.AddObjectToAsset(tex.name, tex);

                if (config.generateMaterials) {
                    var newMaterial = ScopaCore.BuildMaterialForTexture(tex, config);
                    ctx.AddObjectToAsset(tex.name, newMaterial);
                }
            }
            
            // generate atlas sample thumbnail, set as main asset
            int atlasSize = config.GetAtlasSize();
            var atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false, config.linearColorspace  );
            atlas.name = wad.Name;
            atlas.PackTextures(textures.ToArray(), 0, atlasSize);
            atlas.Compress(config.compressTextures);
            ctx.AddObjectToAsset(atlas.name, atlas);
            ctx.SetMainObject(atlas);

        }

 
    }

}