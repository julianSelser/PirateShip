﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;

namespace AlumnoEjemplos.RenderGroup
{
    //clase que define todo el comportamiento base de un barco
    class Barco : TgcMesh
    {
        //esfera que vamos a usar para calcular colisiones
        public TgcBoundingSphere boundingSphere;
        public TgcArrow collisionNormalArrow;

        public float aceleracion = 0f;

        #region CONSTANTES
        public const float COTA_DESACELERACION = 0.01f;
        public const float FACTOR_DESACELERATIVO = 1.015f;
        public const float VELOCIDAD_ROTACION = .7f;
        public const float VELOCIDAD = 300f;
        public const float ACELERACION_MAX = 3f;
        #endregion
 
        //mueve el barco y su boundingspehere en Y; hay que refactorearlo...
        virtual public void flotar()
        {
            float Y = Oceano.alturaMarEnPunto(this.Position.X, this.Position.Z);

            this.Position = new Vector3(this.Position.X, Y - 10, this.Position.Z);

            this.boundingSphere.moveCenter(new Vector3(0, Y - boundingSphere.Position.Y + 50, 0));
        }

        //define un update overrideable para todos los barcos
        virtual public void update()
        {
            //si el usuario quiere ver el bounding sphere...renderizarlo
            if ((bool)GuiController.Instance.Modifiers.getValue("showBoundingBox"))
                this.boundingSphere.render();

            if ((bool)GuiController.Instance.Modifiers.getValue("normales"))
            {
                //Flecha Normal para ver. Solo se actualiza si el barco esta en movimiento
                Vector3 normal = Oceano.normalEnPuntoXZ(this.Position.X, this.Position.Z);

                collisionNormalArrow.PStart = this.Position;
                collisionNormalArrow.PEnd = this.Position + Vector3.Multiply(normal, 200);
                collisionNormalArrow.updateValues();
            }

            this.flotar();
        }

        new public void render() 
        {
            if ((bool)GuiController.Instance.Modifiers.getValue("normales"))
            {
                collisionNormalArrow.render();                
            }

            base.render();
        }

        //cada barco por separada se encarga de actualizarse antes de su render
        public void UpdateRender()
        {
            this.update();
            this.render();
        }

        //metodo que maneja la aceleracion...de mala manera...por ahora...
        public float acelerar(float aceleracionInstantanea) 
        {
            if (aceleracionInstantanea > 0 && aceleracionInstantanea <= ACELERACION_MAX)
                return aceleracion < ACELERACION_MAX ? aceleracion += aceleracionInstantanea : ACELERACION_MAX;

            if (aceleracionInstantanea < 0 && aceleracionInstantanea >= -ACELERACION_MAX)
                return aceleracion > -ACELERACION_MAX ? aceleracion += aceleracionInstantanea : -ACELERACION_MAX;

            throw new Exception("La aceleracion instantanea debe ser: -MAX < aceleracionInstantanea < MAX");
        }

        public float desacelerar() 
        {
            //si aceleracion > 0.01 || -0.01 < aceleracion dividirla hasta que lo este...en ese intervalo la seteamos a cero
            return (aceleracion > COTA_DESACELERACION || aceleracion < -COTA_DESACELERACION) ? aceleracion /= FACTOR_DESACELERATIVO : aceleracion = 0;
        }

        //movimiento lineal en la direccion que apunte el barco con una aceleracion
        public void mover(float aceleracion)
        {
            Vector3 direccion = new Vector3(FastMath.Sin(this.rotation.Y), 0, FastMath.Cos(rotation.Y));

            Vector3 movimiento = direccion * VELOCIDAD * aceleracion * GuiController.Instance.ElapsedTime;

            base.move(movimiento);
            boundingSphere.moveCenter(movimiento);
        }

        //redefine dispose para incluir al boundingsphere
        new public void dispose()
        {
            boundingSphere.dispose();
            base.dispose();
        }

        new public void initData(Mesh d3dMesh, string meshName, TgcMesh.MeshRenderType renderType)
        {           
            collisionNormalArrow = new TgcArrow();
            collisionNormalArrow.BodyColor = Color.Red;
            collisionNormalArrow.HeadColor = Color.Yellow;
            collisionNormalArrow.Thickness = 1f;
            collisionNormalArrow.HeadSize = new Vector2(2, 5);

            base.initData(d3dMesh, meshName, renderType);
        }
    }
}