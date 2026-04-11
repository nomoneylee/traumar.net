# 統計間隔計算研究 (Confidence Intervals)

## 1. Clopper-Pearson (Exact) 方法
### 研究決策
使用 `MathNet.Numerics.Distributions.Beta.InvCDF` 實作。

### 數學原理
對於成功次數 $x$, 樣本數 $n$, 置信水準 $1-\alpha$：
- 下界 $L$：$\text{BetaInv}(\alpha/2; x, n-x+1)$
- 上界 $U$：$\text{BetaInv}(1-\alpha/2; x+1, n-x)$
- 特殊情況：
    - $x=0 \implies L=0$
    - $x=n \implies U=1$

### 決策理由
`MathNet.Numerics` 已包含於專案中，且 Beta 分佈的累積分布函數反函數是 Clopper-Pearson 的標準計算方式，能確保與 R 語言的 `binom.test` 或 `nemsqar` 一致。

## 2. Wilson (Score) 方法
### 研究決策
手動實作 Wilson Score 公式。

### 數學原理
$\hat{p} = x/n$, $z = \Phi^{-1}(1-\alpha/2)$
- $Center = \frac{\hat{p} + \frac{z^2}{2n}}{1 + \frac{z^2}{n}}$
- $Spread = \frac{z \sqrt{\frac{\hat{p}(1-\hat{p})}{n} + \frac{z^2}{4n^2}}}{1 + \frac{z^2}{n}}$
- $L = Center - Spread$
- $U = Center + Spread$

### 決策理由
公式固定且不依賴複雜分布（僅需標準正態分布的分位數），手動實作可精確控制邊界邏輯。

## 3. R 端環境與 `nemsqar`
### 研究決策
R 腳本將透過 `renv::load()` 確保環境一致。

### 決策理由
專案根目錄已有 `renv.lock`，使用 `renv` 可保證產生 Golden Files 的 R 版本與指標邏輯開發時一致。
