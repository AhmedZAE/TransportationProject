using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.LinearAlgebra;

namespace DistributionCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double precision = 0.0001;

        private readonly ObservableCollection<DataSheetRow> _dataRows;


        public MainWindow()
        {
            InitializeComponent();

            _dataRows = new(Enumerable.Range(0, 11).Select(i => new DataSheetRow()));
            McDataGrid.ItemsSource = _dataRows;
        }

        private void McDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            int rowNumber = e.Row.GetIndex() + 1;
            e.Row.Header = rowNumber == McDataGrid.Items.Count ? "Att." : rowNumber;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new();
            for (int i = 0; i < 10; i++)
            {
                _dataRows[i].Data[10] = rnd.Next(100, 300) * 100;
                _dataRows[10].Data[i] = rnd.Next(100, 300) * 100;

                for (int j = 0; j < 10; j++)
                {
                    _dataRows[i].Data[j] = rnd.Next(5, 95) * 10;
                }
            }
            McDataGrid.IsReadOnly = false;
            CalculateBtn.Content = "Calculate";
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (McDataGrid.IsReadOnly)
            {
                for (int i = 0; i < 11; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        _dataRows[i].Data[j] = 0;
                    }
                }
                McDataGrid.IsReadOnly = false;
                CalculateBtn.Content = "Calculate";
                return;
            }

            MatrixBuilder<double> mBuilder = Matrix<double>.Build;
            VectorBuilder<double> vBuilder = Vector<double>.Build;


            var coefficientRowArrays = _dataRows.Take(10).Select(row => row.Data.Take(10));
            var coefficientMatrix = mBuilder.DenseOfRows(coefficientRowArrays);

            var coefficientVectors1 = coefficientMatrix.EnumerateRows();
            var coefficientVectors2 = coefficientMatrix.EnumerateColumns();


            double[] constantVector1Array = _dataRows.Take(10).Select(row => row.Data[10]).ToArray();
            var constantVector1 = vBuilder.Dense(constantVector1Array);

            var constantVector2Array = _dataRows[10].Data.Take(10);
            var constantVector2 = vBuilder.DenseOfEnumerable(constantVector2Array);


            Vector<double> aVector = vBuilder.Dense(10, 0);
            Vector<double> bVector = vBuilder.Dense(10, 1);
            Vector<double> productionVector = constantVector1;
            Vector<double> attractionVector = constantVector2;

            while (true)
            {
                aVector = vBuilder.Dense(GetVariables(coefficientVectors1, bVector, constantVector1));
                bVector = vBuilder.Dense(GetVariables(coefficientVectors2, aVector, constantVector2));

                GetSum(aVector, bVector, out Vector<double> productionVectorCalculated, out Vector<double> attractionVectorCalculated);

                if ((productionVectorCalculated - productionVector).AbsoluteMaximum() < precision && (attractionVectorCalculated - attractionVector).AbsoluteMaximum() < precision)
                {
                    productionVector = productionVectorCalculated;
                    attractionVector = attractionVectorCalculated;
                    break;
                }
                productionVector = productionVectorCalculated;
                attractionVector = attractionVectorCalculated;
            }

            for (int i = 0; i < 10; i++)
            {
                _dataRows[i].Data[10] = productionVector[i];
                _dataRows[10].Data[i] = attractionVector[i];

                for (int j = 0; j < 10; j++)
                {
                    _dataRows[i].Data[j] *= aVector[i] * bVector[j];
                }
            }

            McDataGrid.IsReadOnly = true;
            ((Button)sender).Content = "Clear";
        }


        private static double[] GetVariables(IEnumerable<Vector<double>> coefficientVectors, Vector<double> oldVariableVector, Vector<double> constantVector)
        {
            return coefficientVectors.Select((vector, i) =>
            {
                double denominator = (vector.ToRowMatrix() * oldVariableVector)[0];
                return denominator == 0 ? 0 : constantVector[i] / denominator;
            }).ToArray();
        }


        private void GetSum(Vector<double> aVector, Vector<double> bVector, out Vector<double> productionVector, out Vector<double> attractionVector)
        {
            productionVector = Vector<double>.Build.Dense(10, 0);
            attractionVector = Vector<double>.Build.Dense(10, 0);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    productionVector[i] += _dataRows[i].Data[j] * aVector[i] * bVector[j];
                    attractionVector[j] += _dataRows[i].Data[j] * aVector[i] * bVector[j];
                }
            }
        }

    }
}
