using System;
using System.Text;
using System.Collections.Generic;
using static System.Console;

public class Field {
  private int _size;
  private int _turn;
  // private Dictionary<int, Crane> _cranes = new Dictionary<int, Crane>();
  // private Dictionary<int, Container> _containers = new Dictionary<int, Container>();
  private List<List<int>> _craneMap = new List<List<int>>();
  private List<List<int>> _containerMap = new List<List<int>>();
  private List<List<int>> _ready = new List<List<int>>();
  private List<List<int>> _done = new List<List<int>>();

  public int Turn {
    get {return _turn;}
  }

  public Field(int size, List<List<int>> ready) {
    _size = size;
    _turn = 0;
    _ready = ready;
    
    // for(int i = 0; i < _size; i++) {
    //   for(int j = 0; j < _size; j++) {
    //     if(j == 0) {
    //       _containers[_ready[i][j]] = new Container(_ready[i][j], i, 0);
    //     } else {
    //       _containers[_ready[i][j]] = new Container(_ready[i][j], -1, -1);
    //     }
    //   }
    // }

    for(int i = 0; i < _size; i++) {
      var tmp = new List<int>();
      var tmp2 = new List<int>();
      for(int j = 0; j < _size; j++) {
        tmp.Add(-1);
        tmp2.Add(-1);
      }
      _craneMap.Add(tmp);
      _containerMap.Add(tmp2);
      
      _done.Add(new List<int>());
    }

    for(int i = 0; i < _size; i++) {
      CarryIn(i);
      SetCrane(i, i, 0);
    }
  }

  public int CountInversion(List<int> a) {
    int res = 0;
    for(int i = 0; i < a.Count; i++) {
      for(int j = i + 1; j < a.Count; j++) {
        if(a[i] > a[j]) {
          res++;
        }
      }
    }
    return res;
  }

  public int CalcScore() {
    int inv = 0;
    for(int i = 0; i < _size; i++) {
      inv += CountInversion(_done[i]);
    }

    int wrong = 0;
    for(int i = 0; i < _size; i++) {
      foreach(int x in _done[i]) {
        if(x / _size != i) {
          wrong++;
        }
      }
    }

    int yet = _size * _size;
    for(int i = 0; i < _size; i++) {
      yet -= _done[i].Count;
    }

    return _turn + (100 * inv) + (10000 * wrong) + (1000000 * yet);
  }

  public void CarryIn(int row) {
    if(!(0 <= row && row < _size)) throw new Exception("範囲外の行");
    if(_ready[row].Count == 0) throw new Exception($"待機列{row}にコンテナがありません");
    if(_containerMap[row][0] != -1) throw new Exception($"搬入口({row},{0})に別のコンテナが存在します");

    _containerMap[row][0] = _ready[row][0];
    _ready[row].RemoveAt(0);
  }

  public void CarryOut() {
    for(int i = 0; i < _size; i++) {
      if(_containerMap[i][_size - 1] != -1) {
        _done[i].Add(_containerMap[i][_size - 1]);
        _containerMap[i][_size - 1] = -1;
      }
    }
  }

  private void SetCrane(int id, int y, int x) {
    if(!(0 <= y && y < _size && 0 <= x && x < _size)) throw new Exception("範囲外");
    _craneMap[y][x] = id;
  }

  public (int Y, int X) GetCranePos(int id) {
    for(int i = 0; i < _size; i++) {
      for(int j = 0; j < _size; j++) {
        if(_craneMap[i][j] == id) {
          return (i, j);
        }
      }
    }

    throw new Exception($"クレーン{id}はフィールドに存在しません");
  }

  public (int Y, int X) GetContainerPos(int id) {
    for(int i = 0; i < _size; i++) {
      for(int j = 0; j < _size; j++) {
        if(_containerMap[i][j] == id) {
          return (i, j);
        }
      }
    }

    throw new Exception($"コンテナ{id}はフィールドに存在しません");
  }

  public override string ToString() {
    var tmp = new StringBuilder();
    tmp.Append($"フィールド\n");

    // tmp.Append($"クレーン一覧...[");
    // foreach(var c in _cranes) {
    //   tmp.Append($"{c},\n");
    // }
    // tmp.Append($"]\n");

    // tmp.Append($"コンテナ一覧...[");
    // foreach(var c in _containers) {
    //   tmp.Append($"{c},\n");
    // }
    // tmp.Append($"]\n");

    tmp.Append($"マップ(クレーン、コンテナ)\n");
    for(int i = 0; i < _craneMap.Count; i++) {
      for(int j = 0; j < _craneMap[i].Count; j++) {
        tmp.Append($"({_craneMap[i][j]},{_containerMap[i][j]})".PadLeft(8));
      }
      tmp.Append("\n");
    }

    tmp.Append("待機\n");
    for(int i = 0; i < _size; i++) {
      tmp.Append($"<- [");
      foreach(var x in _ready[i]) {
        tmp.Append($"{x},");
      }
      tmp.Append("]\n");
    }

    tmp.Append("完了\n");
    for(int i = 0; i < _size; i++) {
      tmp.Append($"[");
      foreach(var x in _done[i]) {
        tmp.Append($"{x},");
      }
      tmp.Append("] <-\n");
    }

    return tmp.ToString();
  }
}

public class Container {
  private int _id;
  // private int _y;
  // private int _x;

  public int ID {
    get {return _id;}
  }

  // public int Y {
  //   get {return _y;}
  // }

  // public int X {
  //   get {return _x;}
  // }

  public Container(int id, int y, int x) {
    _id = id;
    // _y = y;
    // _x = x;
  }

  public override string ToString() {
    var tmp = new StringBuilder();
    tmp.Append($"コンテナ{_id} ");
    // tmp.Append($"({_y}, {_x}) ");
    return tmp.ToString();
  }
}

public class Crane {
  private static bool _instance = false;

  private int _id;
  // private int _y;
  // private int _x;
  private bool _isBan;
  private bool _isLarge;
  private Container _grabbedContainer;

  public int ID {
    get {return _id;}
  }

  // public int Y {
  //   get {return _y;}
  // }

  // public int X {
  //   get {return _x;}
  // }

  public bool IsBan {
    get {return _isBan;}
  }

  public bool IsLarge {
    get {return _isLarge;}
  }

  public Container GrabbedContainer {
    get {return _grabbedContainer;}
  }

  public Crane(int id, int y, int x) {
    _id = id;
    // _y = y;
    // _x = x;
    _isBan = false;
    _isLarge = !(_instance);
    _instance = true;
  }

  public override string ToString() {
    var tmp = new StringBuilder();
    tmp.Append($"{(_isLarge ? "大" : "小")}クレーン{_id} ");
    // tmp.Append($"({_y}, {_x}) ");
    tmp.Append($"{(_isBan ? "破壊済み" : "使用可能")} ");
    tmp.Append($"掴んでいるコンテナ...{(_grabbedContainer is null ? "なし" : _grabbedContainer.ID.ToString())}");
    return tmp.ToString();
  }
}

public class MainClass
{
  public MainClass() {
    int n = int.Parse(ReadLine());
    var ready = new List<List<int>>();

    for(int i = 0; i < n; i++) {
      int[] a = ReadLine().Split().Select(int.Parse).ToArray();
      var tmp = new List<int>();
      foreach(int x in a) {
        tmp.Add(x);
      }
      ready.Add(tmp);
    }

    var f = new Field(n, ready);
    WriteLine(f.ToString());
  }

  public static int Main(string[] args) {
    new MainClass();
    return 0;
  }
}
