using System;
using System.Text;
using System.Collections.Generic;
using static System.Console;

public class Field {
  private int _size;
  private int _turn;
  // private Dictionary<int, Crane> _cranes = new Dictionary<int, Crane>();
  // private Dictionary<int, Container> _containers = new Dictionary<int, Container>();
  private List<List<char>> _processes = new List<List<char>>();
  private List<List<int>> _craneMap = new List<List<int>>();
  private List<List<int>> _containerMap = new List<List<int>>();
  private List<int> _grabbedContainer = new List<int>();
  private List<List<int>> _ready = new List<List<int>>();
  private List<List<int>> _done = new List<List<int>>();

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
      _grabbedContainer.Add(-1);
      _processes.Add(new List<char>());
    }
  }

  public int Size {
    get {return _size;}
  }

  public int Turn {
    get {return _turn;}
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
    // for(int i = 0; i < _size; i++) {
    //   if(_grabbedContainer[i] == id) {
    //     return GetCranePos(i);
    //   }
    // }

    for(int i = 0; i < _size; i++) {
      for(int j = 0; j < _size; j++) {
        if(_containerMap[i][j] == id) {
          return (i, j);
        }
      }
    }

    throw new Exception($"コンテナ{id}はフィールドに存在しません");
  }

  public void MoveCrane(int id, char direction) {
    (int cy, int cx) = GetCranePos(id);
    (int ey, int ex) = (cy, cx);
    switch(direction) {
      case 'U':
        ey--;
        break;
      case 'D':
        ey++;
        break;
      case 'L':
        ex--;
        break;
      case 'R':
        ex++;
        break;
      default:
        throw new Exception("方向の指定方法が間違っています");
    }

    if(!(0 <= ey && ey < _size && 0 <= ex && ex < _size)) {
      throw new Exception($"クレーン{id}が範囲外に移動しました");
    }

    if(_craneMap[ey][ex] != -1) {
      throw new Exception($"クレーン{id}がクレーン{_craneMap[ey][ex]}と衝突しました");
    }

    if(id != 0 && _grabbedContainer[id] != -1 && _containerMap[ey][ex] != -1) {
      throw new Exception($"小クレーン{id}はコンテナを掴んだ状態で別のコンテナが存在するマスに移動出来ません");
    }

    _craneMap[ey][ex] = id;
    _craneMap[cy][cx] = -1;

    if(cx == 0 && _grabbedContainer[id] != -1 && _ready[cy].Count >= 1) {
      CarryIn(cy);
    }
  }

  public void Ban(int id) {
    if(_grabbedContainer[id] != -1) {
      throw new Exception($"コンテナを掴んだ状態でクレーン{id}は爆破出来ません");
    }

    (int y, int x) = GetCranePos(id);
    _craneMap[y][x] = -1;
  }

  public void Grab(int id) {
    if(_grabbedContainer[id] != -1) {
      throw new Exception($"既にクレーン{id}はコンテナ{_grabbedContainer[id]}を掴んでいます");
    }

    (int y, int x) = GetCranePos(id);
    if(_containerMap[y][x] == -1) {
      throw new Exception($"クレーン{id}が現在いるマスにコンテナが存在しません");
    }

    _grabbedContainer[id] = _containerMap[y][x];
    _containerMap[y][x] = -1;
  }

  public void Drop(int id) {
    if(_grabbedContainer[id] == -1) {
      throw new Exception($"掴んでいるコンテナがないためクレーン{id}に対して操作を行えません");
    }

    (int y, int x) = GetCranePos(id);
    if(_containerMap[y][x] != -1) {
      throw new Exception($"クレーン{id}の現在いるマスに別のコンテナが既に存在するためコンテナを置けません");
    }

    _containerMap[y][x] = _grabbedContainer[id];
    _grabbedContainer[id] = -1;
  }

  // private bool isCollide(in List<List<char>> processes, int turn) {
  //   HashSet<(int Y, int X)> afterMove;
  //   for(int i = 0; i < _size; i++) {
  //     if(processes[i][turn] == 'B') continue;
  //     (int cy, int cx) = GetCranePos(i);

  //     int ey, ex;
  //     switch(processes[i][turn]) {
  //       case 'P':
  //       case 'Q':
  //         (ey, ex) = (cy, cx);
  //         break;
  //       case 'U':
  //         (ey, ex) = (cy - 1, cx);
  //         break;
  //       case 'D':
  //         (ey, ex) = (cy + 1, cx);
  //         break;
  //       case 'L':
  //         (ey, ex) = (cy, cx - 1);
  //         break;
  //       case 'R':
  //         (ey, ex) = (cy, cx + 1);
  //         break;
  //     }

  //     if(afterMove.Contains((ey, ex))) {
  //       return false;
  //     }
  //   }
  //   return true;
  // }

  private bool NextPermutation(List<int> a) {
    if(a.Count <= 1) return false;

    int k = a.Count - 2;
    while(k >= 0 && a[k] >= a[k + 1]) {
      k--;
    }

    if(k < 0) {
      a.Reverse();
      return false;
    }

    int l = a.Count - 1;
    while(l > k && a[l] <= a[k]) {
      l--;
    }

    int tmp = a[k];
    a[k] = a[l];
    a[l] = tmp;

    a.Reverse(k + 1, a.Count - (k + 1));
    return true;
  }

  public bool IsValidTurn(in List<List<char>> processes, int turn) {
    var before = new List<(int Y, int X)>();
    var after = new List<(int Y, int X)>();
    for(int i = 0; i < _size; i++) {
      before.Add(GetCranePos(i));
      after.Add(processes[i][turn] switch {
        'P' => before[i],
        'Q' => before[i],
        '.' => before[i],
        'B' => (-1, -1),
        'U' => (before[i].Y - 1, before[i].X),
        'D' => (before[i].Y + 1, before[i].X),
        'L' => (before[i].Y, before[i].X - 1),
        'R' => (before[i].Y, before[i].X + 1),
        _ => throw new Exception("不正な入力"),
      });
    }

    for(int i = 0; i < _size; i++) {
      if((after[i].Y == -1 || after[i].X == -1) && after[i] != (-1, -1)) {
        return false;
      }

      for(int j = i + 1; j < _size; j++) {
        if(before[i] == after[j] && after[i] == before[j]) {
          return false;
        }
        if(after[i] == after[j]) {
          return false;
        }
      }
    }

    return true;
  }

  public void Operate(List<List<char>> processes) {
    if(processes.Count != _size) {
      throw new Exception("操作列の数が間違っています");
    }

    int maxTurn = 0;
    for(int i = 0; i < _size; i++) {
      if(processes[i].Count > maxTurn) {
        maxTurn = processes[i].Count;
      }
    }
    for(int i = 0; i < _size; i++) {
      while(processes[i].Count < maxTurn) {
        processes[i].Add('.');
      }
    }

    for(int t = 0; t < maxTurn; t++) {
      for(int c = 0; c < _size; c++) {
        switch(processes[c][t]) {
          case 'P':
            Grab(c);
            break;
          case 'Q':
            Drop(c);
            break;
          case 'U':
          case 'D':
          case 'L':
          case 'R':
            MoveCrane(c, processes[c][t]);
            break;
          case 'B':
            Ban(c);
            break;
        }
      }
    }

    for(int i = 0; i < _size; i++) {
      _processes[i].AddRange(processes[i]);
    }
  }

  public string GetAnswer() {
    var tmp = new StringBuilder();
    for(int i = 0; i < _size; i++) {
      foreach(char c in _processes[i]) {
        tmp.Append(c);
      }
      tmp.Append("\n");
    }
    return tmp.ToString();
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
        tmp.Append($"(({_craneMap[i][j]},{(_craneMap[i][j] != -1 ? _grabbedContainer[_craneMap[i][j]] : -1)}){_containerMap[i][j]})".PadLeft(12));
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
  public static void Init(ref Field f) {
    var processes = new List<List<char>>();
    for(int i = 0; i < f.Size; i++) {
      processes.Add(new List<char>());
    }
    processes[0].AddRange("PRQLPRRQLLPRRRQRDD.");
    processes[1].AddRange("PRRRQLLLPRRQLLPRQLD");
    processes[2].AddRange("PRRRQLLLPRRQLLPRQ..");
    processes[3].AddRange("PRRRQLLLPRRQLLPRQRU");
    processes[4].AddRange("PRRRQLLLPRRQLLPRQB.");

    f.Operate(processes);
  }

  public static int Main(string[] args) {
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
    Init(ref f);
    WriteLine(f.ToString());
    WriteLine(f.GetAnswer());

    return 0;
  }
}
