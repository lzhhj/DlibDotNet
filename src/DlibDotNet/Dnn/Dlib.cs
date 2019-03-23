﻿using System;
using System.Collections.Generic;
using System.Linq;
using DlibDotNet.Dnn;
using DlibDotNet.Extensions;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    /// <summary>
    /// Provides the methods of dlib.
    /// </summary>
    public static partial class Dlib
    {

        #region Methods

        #region AssignPixel

        public static Matrix<double> TestObjectDetectionFunction<T>(LossMmod detector,
                                                                    IEnumerable<Matrix<T>> images,
                                                                    IEnumerable<IEnumerable<MModRect>> truthDets,
                                                                    TestBoxOverlap overlapTester = null,
                                                                    double adjustThreshold = 0,
                                                                    TestBoxOverlap overlapIgnoreTester = null)
           where T : struct
        {
            if (detector == null)
                throw new ArgumentNullException(nameof(detector));
            if (images == null)
                throw new ArgumentNullException(nameof(images));
            if (truthDets == null)
                throw new ArgumentNullException(nameof(truthDets));

            detector.ThrowIfDisposed();
            images.ThrowIfDisposed();
            truthDets.ThrowIfDisposed();

            var disposeOverlapTester = overlapTester == null;
            var disposeOverlapIgnoreTester = overlapIgnoreTester == null;
            List<StdVector<MModRect>> listOfVectorOfMModRect = null;

            try
            {
                if (disposeOverlapTester)
                    overlapTester = new TestBoxOverlap();
                if (disposeOverlapIgnoreTester)
                    overlapIgnoreTester = new TestBoxOverlap();

                listOfVectorOfMModRect = truthDets.Select(r => new StdVector<MModRect>(r)).ToList();

                using (var matrixVector = new StdVector<Matrix<T>>(images))
                using (var detsVector = new StdVector<StdVector<MModRect>>(listOfVectorOfMModRect))
                {
                    var type = detector.NetworkType;
                    Matrix<T>.TryParse<T>(out var elementTypes);
                    var matrix = images.FirstOrDefault();
                    var ret = NativeMethods.test_object_detection_function_net(type,
                                                                               detector.NativePtr,
                                                                               elementTypes.ToNativeMatrixElementType(),
                                                                               matrixVector.NativePtr,
                                                                               matrix.TemplateRows,
                                                                               matrix.TemplateColumns,
                                                                               detsVector.NativePtr,
                                                                               overlapTester.NativePtr,
                                                                               adjustThreshold,
                                                                               overlapIgnoreTester.NativePtr,
                                                                               out var result);
                    switch (ret)
                    {
                        case NativeMethods.ErrorType.MatrixElementTypeNotSupport:
                            throw new ArgumentException($"{elementTypes} is not supported.");
                        case NativeMethods.ErrorType.DnnNotSupportNetworkType:
                            throw new NotSupportNetworkTypeException(type);
                    }

                    return new Matrix<double>(result, 1, 3);
                }
            }
            finally
            {
                if (listOfVectorOfMModRect != null)
                    foreach (var stdVector in listOfVectorOfMModRect)
                        stdVector?.Dispose();
                if (disposeOverlapTester)
                    overlapTester?.Dispose();
                if (disposeOverlapIgnoreTester)
                    overlapIgnoreTester?.Dispose();
            }
        }

        #endregion

        #endregion

    }

}