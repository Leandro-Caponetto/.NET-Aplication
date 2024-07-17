using System;
using System.Collections.Generic;
using System.Text;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using Core.RequestsHTTP.Models.TiendaNube;
using System.Globalization;

namespace Core.V1.TiendaNube.GetBranchOffices
{
    public class Algorith_calc_volume
    {

        class Dimension
        {
            public int[] _tX;
            public int _q;

            public Dimension(ItemModel item)
            {
                _tX = new int[3] { (int)item.dimensions.width, (int)item.dimensions.depth, (int)item.dimensions.height };
                _q = (int)item.quantity;
                sort();
            }

            public Dimension(ProductsModel prod)
            {
                _tX = new int[3] {
                    (int)Convert.ToDouble(prod.width, new CultureInfo("en-US")),
                    (int)Convert.ToDouble(prod.depth, new CultureInfo("en-US")),
                    (int)Convert.ToDouble(prod.height, new CultureInfo("en-US"))
                };
                _q = prod.quantity;
                sort();
            }

            public Dimension(Dimension from)
            {
                _tX = new int[from._tX.Length];
                for (int i = 0; i < _tX.Length; ++i)
                    _tX[i] = from._tX[i];
                _q = 1;
            }

            Dimension(Dimension from, int q)
            {
                _tX = new int[from._tX.Length];
                for (int i = 0; i < _tX.Length; ++i)
                    _tX[i] = from._tX[i];

                _tX[_tX.Length - 1] *= q;
                sort();
                _q = 1;
            }

            public void add(Dimension add)
            {
                int imx = _tX.Length - 1;
                _tX[imx] += add._tX[imx];

                for (int i = 0; i < imx; ++i)
                {
                    if (_tX[i] < add._tX[i])
                        _tX[i] = add._tX[i];
                }

                sort();
            }

            void sort()
            {
                for (int i = 1; i < _tX.Length; ++i)
                {
                    if (_tX[i - 1] < _tX[i])
                    {
                        int xmax = _tX[i];
                        _tX[i] = _tX[i - 1];
                        _tX[i - 1] = xmax;
                        if (1 < i)
                            i -= 2;
                    }
                }
            }

            public int unit_volume()
            {
                int v = 1;
                for (int i = 0; i < _tX.Length; ++i)
                    v *= _tX[i];
                return v;
            }

            public Dimension stack()
            {
                Dimension newDim = null;

                if (1 < _q)
                {
                    int imx = _tX.Length - 1;

                    if (_tX[imx] <= 0)
                        return newDim;

                    int qX = _tX[1] / _tX[imx];
                    if (_q <= qX)
                    {
                        _tX[imx] *= _q;
                        _q = 1;
                    }
                    else
                    { // 9   q2 = 3; q1 = 2; q = 1 
                        int[] q = new int[3];
                        q[2] = 1 < qX ? _q / qX : (int)Math.Ceiling(Math.Pow(_q, 1.0 / 3.0));
                        q[1] = 1 < qX ? q[2] < _q ? _q / q[2] : 1 : (int)Math.Ceiling(Math.Sqrt(q[2]));
                        q[0] =   q[2] * q[1] < _q ? _q / (q[2] * q[1]) : 1;

                        for (int i = 1; i < q.Length; ++i)
                        {
                            if (q[i] < q[i -1])
                            {
                                int qa = q[i];
                                q[i] = q[i - 1];
                                q[i - 1] = qa;
                                if (1 < i)
                                    i -= 2;
                            }
                        }

                        int r = q[2] * q[1] * q[0] < _q ? _q % (q[2] * q[1] * q[0]) : 0;
                        if (0 < r)
                            newDim = new Dimension(this, r);

                        for (int i = 0; i < _tX.Length; ++i)
                            _tX[i] *= q[i];
                        _q = 1;
                    }
                    sort();
                }

                return newDim;
            }

        }

        List<Dimension> _ld;
        Dimension _total;

        public Algorith_calc_volume()
        {
            _ld = new List<Dimension>();
        }

        private void stack_add(Dimension idim)
        {
            int vol = idim.unit_volume();
            if (vol == 0)
                _ld.Add(idim);
            else
            {
                bool bInserted = false;
                for (int i = 0; i < _ld.Count && !bInserted; ++i)
                {
                    int ivol = _ld[i].unit_volume();
                    if (ivol < vol)
                    {
                        _ld.Insert(i, idim);
                        bInserted = true;
                    }
                    else if (ivol == vol)
                    {
                        _ld[i]._q += idim._q;
                        bInserted = true;
                    }

                }
                if (!bInserted)
                    _ld.Add(idim);
            }
        }

        private int[] stack_end()
        {
            for (int i = 0; i < _ld.Count; ++i)
            {
                Dimension newDim = _ld[i].stack();
                if (i == 0)
                    _total = new Dimension(_ld[i]);
                else
                    _total.add(_ld[i]);

                if (newDim != null)
                {
                    int nvol = newDim.unit_volume();
                    bool bInserted = false;
                    for (int j = i + 1; j < _ld.Count && !bInserted; ++j)
                    {
                        int jvol = _ld[j].unit_volume();
                        if (jvol < nvol)
                        {
                            _ld.Insert(j + 1, newDim);
                            bInserted = true;
                        }
                        else if (jvol == nvol)
                        {
                            _ld[j]._q += newDim._q;
                            bInserted = true;
                        }
                    }
                    if (!bInserted)
                        _ld.Add(newDim);
                }
            }
            return _total._tX;
        }

        public int[] Stack(IEnumerable<ItemModel> items)
        {
            _ld.Clear();
            foreach (ItemModel item in items)
            {
                if (item.quantity <= 0 || item.dimensions == null)
                    continue;

                Dimension idim = new Dimension(item);
                stack_add(idim);
            }

            return stack_end();
        }

        public int[] Stack(IEnumerable<ProductsModel> products)
        {
            _ld.Clear();
            foreach (ProductsModel prod in products)
            {
                if (prod.quantity <= 0)
                    continue;

                Dimension idim = new Dimension(prod);
                stack_add(idim);
            }

            return stack_end();
        }
    }
}
