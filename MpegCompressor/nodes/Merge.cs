using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class Merge : Node {
        public enum Method { Multiply, Add, Subtract, Divide };
        private static string[] options
            = new string[4] {
                "Multiply",
                "Add",
                "Subtract",
                "Divide" };
        private Method method;

        public Merge() {
            method = 0;
            rename("Merge");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new Property(false, false);
            p.createChoices(options, (int)method, "merge operator");
            p.eValueChanged += e_OpChanged;
            properties["mergeMethod"] = p;

            p = new Property(false, false);
            p.createButton("Swap", "Swap inputs A and B");
            p.eValueChanged += e_SwapInputs;
            properties["btnSwap"] = p;

            properties.Add("inColorA", new Property(true, false));
            properties.Add("inColorB", new Property(true, false));
            properties.Add("outColor", new Property(false, true));
        }

        private void e_SwapInputs(object sender, EventArgs e) {
            Address temp = properties["inColorA"].input;
            properties["inColorA"].input = properties["inColorB"].input;
            properties["inColorB"].input = temp;
            soil();
        }

        public void setMethod(Method newMethod) {
            method = newMethod;
            properties["mergeMethod"].setSelection((int)newMethod);
            soil();
            
        }

        private void e_OpChanged(object sender, EventArgs e) {
            setMethod((Method)properties["mergeMethod"].getSelection());
        }

        protected override void clean() {
            base.clean();

            Address upstreamA = properties["inColorA"].input;
            if (upstreamA == null) {
                return;
            }
            Address upstreamB = properties["inColorB"].input;
            if (upstreamB == null) {
                return;
            }

            state = upstreamA.node.getData(upstreamA.port);
            if (state == null) {
                return;
            }
            DataBlob stateB = upstreamB.node.getData(upstreamB.port);
            if (stateB == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Image || state.bmp == null) {
                state = null;
                return;
            }
            if (stateB.type != DataBlob.Type.Image || stateB.bmp == null) {
                stateB = null;
                return;
            }

            //check resolutions match
            if (state.imageWidth != stateB.imageWidth || state.imageHeight != stateB.imageHeight) {
                return;
            }

            state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), state.bmp.PixelFormat);
            merge(stateB.bmp);
        }

        public void merge(Bitmap B) {
            BitmapData bmpDataA = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);
            BitmapData bmpDataB = B.LockBits(
                                new Rectangle(0, 0, B.Width, B.Height),
                                ImageLockMode.ReadWrite,
                                B.PixelFormat);

            IntPtr ptrA = bmpDataA.Scan0;
            IntPtr ptrB = bmpDataB.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpDataA.Stride) * bmpDataA.Height;
            byte[] rgbValuesA = new byte[nBytes];
            byte[] rgbValuesB = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptrA, rgbValuesA, 0, nBytes);
            System.Runtime.InteropServices.Marshal.Copy(ptrB, rgbValuesB, 0, nBytes);

            int Ar, Ag, Ab;
            int Br, Bg, Bb;

            int pixel;
            switch(method) {
                case Method.Multiply:
                    for (int y = 0; y < bmpDataA.Height; y++) {
                        for (int x = 0; x < bmpDataA.Width; x++) {
                            pixel = y * bmpDataA.Stride + x * 3;
                            Ar = rgbValuesA[pixel + 2];
                            Ag = rgbValuesA[pixel + 1];
                            Ab = rgbValuesA[pixel];
                            Br = rgbValuesB[pixel + 2];
                            Bg = rgbValuesB[pixel + 1];
                            Bb = rgbValuesB[pixel];
                            rgbValuesA[pixel + 2] = (byte)(Ar * Br / 255);
                            rgbValuesA[pixel + 1] = (byte)(Ag * Bg / 255);
                            rgbValuesA[pixel + 0] = (byte)(Ab * Bb / 255);
                        }
                    }
                    break;
                case Method.Divide:
                    for (int y = 0; y < bmpDataA.Height; y++) {
                        for (int x = 0; x < bmpDataA.Width; x++) {
                            pixel = y * bmpDataA.Stride + x * 3;
                            Ar = rgbValuesA[pixel + 2];
                            Ag = rgbValuesA[pixel + 1];
                            Ab = rgbValuesA[pixel];
                            Br = rgbValuesB[pixel + 2];
                            Bg = rgbValuesB[pixel + 1];
                            Bb = rgbValuesB[pixel];
                            rgbValuesA[pixel + 2] = (byte)(Math.Min(255, (double)Ar / Br * 255.0));
                            rgbValuesA[pixel + 1] = (byte)(Math.Min(255, (double)Ag / Bg * 255.0));
                            rgbValuesA[pixel + 0] = (byte)(Math.Min(255, (double)Ab / Bb * 255.0));
                        }
                    }
                    break;
                case Method.Add:
                    for (int y = 0; y < bmpDataA.Height; y++) {
                        for (int x = 0; x < bmpDataA.Width; x++) {
                            pixel = y * bmpDataA.Stride + x * 3;
                            Ar = rgbValuesA[pixel + 2];
                            Ag = rgbValuesA[pixel + 1];
                            Ab = rgbValuesA[pixel];
                            Br = rgbValuesB[pixel + 2];
                            Bg = rgbValuesB[pixel + 1];
                            Bb = rgbValuesB[pixel];
                            rgbValuesA[pixel + 2] = (byte)(Math.Min(Ar + Br, 255));
                            rgbValuesA[pixel + 1] = (byte)(Math.Min(Ag + Bg, 255));
                            rgbValuesA[pixel + 0] = (byte)(Math.Min(Ab + Bb, 255));
                        }
                    }
                    break;
                case Method.Subtract:
                    for (int y = 0; y < bmpDataA.Height; y++) {
                        for (int x = 0; x < bmpDataA.Width; x++) {
                            pixel = y * bmpDataA.Stride + x * 3;
                            Ar = rgbValuesA[pixel + 2];
                            Ag = rgbValuesA[pixel + 1];
                            Ab = rgbValuesA[pixel];
                            Br = rgbValuesB[pixel + 2];
                            Bg = rgbValuesB[pixel + 1];
                            Bb = rgbValuesB[pixel];
                            rgbValuesA[pixel + 2] = (byte)(Math.Max(Ar - Br, 0));
                            rgbValuesA[pixel + 1] = (byte)(Math.Max(Ag - Bg, 0));
                            rgbValuesA[pixel + 0] = (byte)(Math.Max(Ab - Bb, 0));
                        }
                    }
                    break;
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValuesA, 0, ptrA, nBytes);

            state.bmp.UnlockBits(bmpDataA);
            B.UnlockBits(bmpDataB);
        }
    }
}
