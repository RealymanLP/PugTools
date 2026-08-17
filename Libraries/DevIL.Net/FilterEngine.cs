/*
* Copyright (c) 2012 Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using DevIL.Unmanaged;

namespace DevIL {
  public class FilterEngine {

    public static SamplingFilter SamplingFilter {
      get {
        return ILU.GetSamplingFilter();
      }
      set {
        ILU.SetSamplingFilter(value);
      }
    }

    public static bool Alienify(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Alienify();
    }

    public static bool BlurAverage(Image image, int iterations) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.BlurAverage(iterations);
    }

    public static bool BlurGaussian(Image image, int iterations) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.BlurGaussian(iterations);
    }

    public static bool BuildMipMaps(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.BuildMipMaps();
    }

    public static bool Contrast(Image image, float contrast) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Contrast(contrast);
    }

    public static bool Convolution(Image image, int[] matrix, int scale, int bias) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Convolution(matrix, scale, bias);
    }

    public static bool EdgeDetectE(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.EdgeDetectE();
    }

    public static bool EdgeDetectP(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.EdgeDetectP();
    }

    public static bool EdgeDetectS(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.EdgeDetectS();
    }

    public static bool Emboss(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Emboss();
    }

    public static bool Equalize(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Equalize();
    }

    public static bool GammaCorrect(Image image, float gamma) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.GammaCorrect(gamma);
    }

    public static bool InvertAlpha(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.InvertAlpha();
    }

    public static bool Negative(Image image) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Negative();
    }

    public static bool Noisify(Image image, float tolerance) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Noisify(tolerance);
    }

    public static bool Pixelize(Image image, int pixelSize) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Pixelize(pixelSize);
    }

    public static bool Saturate(Image image, float saturation) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Saturate(saturation);
    }

    public static bool Saturate(Image image, float red, float green, float blue, float saturation) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Saturate(red, green, blue, saturation);
    }

    public static bool Sharpen(Image image, float factor, int iterations) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Sharpen(factor, iterations);
    }

    public static bool Wave(Image image, float angle) {
      if (image == null || !image.IsValid) {
        return false;
      }

      IL.BindImage(image.ImageID);
      return ILU.Wave(angle);
    }

  }
}
