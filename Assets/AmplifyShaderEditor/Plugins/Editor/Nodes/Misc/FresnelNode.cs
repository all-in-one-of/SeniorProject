// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
// http://kylehalladay.com/blog/tutorial/2014/02/18/Fresnel-Shaders-From-The-Ground-Up.html
// http://http.developer.nvidia.com/CgTutorial/cg_tutorial_chapter07.html

using System;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Fresnel", "Misc", "Simple Fresnel effect" )]
	public sealed class FresnelNode : ParentNode
	{
		private const string WorldDirVarStr = "worldViewDir";

		private const string FresnedDotVar = "fresnelDotVal";
		private const string FresnedFinalVar = "fresnelFinalVal";

		private const string FresnesDotOp = "float {0} = dot( {1},{2} );";
		private const string FresnesFinalOp = "float {0} = {1};";

		private int m_cachedPropertyId = -1;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT3, false, "Normal" );
			AddInputPort( WirePortDataType.FLOAT, false, "Bias" );
			AddInputPort( WirePortDataType.FLOAT, false, "Scale" );
			AddInputPort( WirePortDataType.FLOAT, false, "Power" );
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_useInternalPortData = true;
			m_drawPreviewAsSphere = true;
			m_inputPorts[ 2 ].FloatInternalData = 1;
			m_inputPorts[ 3 ].FloatInternalData = 5;
			m_previewShaderGUID = "240145eb70cf79f428015012559f4e7d";
		}

		public override void SetPreviewInputs()
		{
			base.SetPreviewInputs();

			if ( m_cachedPropertyId == -1 )
				m_cachedPropertyId = Shader.PropertyToID( "_Connected" );

			PreviewMaterial.SetFloat( m_cachedPropertyId, ( m_inputPorts[ 0 ].IsConnected ? 1 : 0 ) );
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			dataCollector.AddToInput( m_uniqueId, UIUtils.GetInputDeclarationFromType( m_currentPrecisionType, AvailableSurfaceInputs.WORLD_POS ), true );
			dataCollector.AddToLocalVariables( m_uniqueId, m_currentPrecisionType, WirePortDataType.FLOAT3, WorldDirVarStr, "normalize( UnityWorldSpaceViewDir( " + Constants.InputVarStr + ".worldPos ) )" );

			string normal = string.Empty;
			if ( m_inputPorts[ 0 ].IsConnected )
			{
				normal = m_inputPorts[ 0 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT3, ignoreLocalvar, true );
			}
			else
			{
				dataCollector.AddToInput( m_uniqueId, UIUtils.GetInputDeclarationFromType( m_currentPrecisionType, AvailableSurfaceInputs.WORLD_NORMAL ), true );
				dataCollector.AddToInput( m_uniqueId, Constants.InternalData, false );
				normal = GeneratorUtils.GenerateWorldNormal( ref dataCollector, m_uniqueId );
				//string normalWorld = "WorldNormalVector( " + Constants.InputVarStr + ", float3( 0, 0, 1 ) );";
				//dataCollector.AddToLocalVariables( m_uniqueId, "float3 worldNormal = "+ normalWorld );
				//normal = "worldNormal";
				//dataCollector.ForceNormal = true;
			}

			string bias = m_inputPorts[ 1 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, ignoreLocalvar, true );
			string scale = m_inputPorts[ 2 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, ignoreLocalvar, true );
			string power = m_inputPorts[ 3 ].GenerateShaderForOutput( ref dataCollector, WirePortDataType.FLOAT, ignoreLocalvar, true );

			string ndotl = "dot( "+ normal + ", "+ WorldDirVarStr + " )";

			string fresnalFinalVar = FresnedFinalVar + m_uniqueId;
			dataCollector.AddToLocalVariables( m_uniqueId, string.Format( FresnesFinalOp, fresnalFinalVar, string.Format( "({0} + {1}*pow( 1.0 - {2} , {3}))", bias, scale, ndotl, power ) ) );
			return CreateOutputLocalVariable( 0, fresnalFinalVar, ref dataCollector );
		}
	}
}
