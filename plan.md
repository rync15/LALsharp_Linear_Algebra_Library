## Plan: Span-First C# 數值計算函式庫全量規劃（Refinement v1）

以 .NET 8 (x64 Windows/Linux) 為目標，採用「公開 API 以 Span<T>/ReadOnlySpan<T> 為主、內部僅在已量測瓶頸導入 unsafe + SIMD + 多執行緒」的雙層架構；功能面以 開發計畫與規格書.md 文件為主體全量覆蓋，並以 Numerical methods_algorithms and tools in C#.pdf 和 Algebra_Topology_Dierential Calculus and Optimization Theory For Computer Science and Machine Learning.pdf（檔名沿用 Dierential）補齊缺項。

### Scope 與非目標
1. **In-scope**: 張量運算核心（含 FFT 與排序）、線性代數核心（含稀疏矩陣基礎與距離度量）、ODE 求解器（含剛性求解）、共用數值工具（最佳化、積分、統計與隨機分佈）、NumPy 概念的 C# API 映射。
2. **Out-of-scope**: 獨立且純粹的拓樸理論或圖論模組（依 Rule 17 僅取數值計算相關）、無量測證據的全面 unsafe 化、視覺化功能。

## 工作分解結構
#### 1. W0 需求與追溯基線
1.1 建立功能追溯矩陣（開發計畫與規格書.md + Numerical methods_algorithms and tools in C#.pdf + Algebra_Topology_Dierential Calculus and Optimization Theory For Computer Science and Machine Learning.pdf（檔名沿用 Dierential）補強，包含新擴充的 FFT、BFGS 最佳化、Brent 尋根等）。
1.2 建立 32 條規則的可驗證條款與審查清單。
1.3 定義包含/排除邊界與延後清單。
1.4 Deliverables: `TraceabilityMatrix.md`, `RuleComplianceChecklist.md`, `ScopeBoundary.md`。
1.5 DoD: 目標功能均有唯一 ID，且可對應到測試與實作節點。

#### 2. W1 架構與資料模型定稿
2.1 分層架構：Public API（Span/ReadOnlySpan）/ Safe Kernel（Span-first）/ Hot Kernel（unsafe）/ Scheduling & Memory。
2.2 資料模型：向量、矩陣、張量、stride view、工作區緩衝，兼容線代與 ODE。
2.3 基礎介面：支援零複製 Padding 策略視圖與稀疏矩陣（CSR/CSC）基礎介面定義。
2.4 記憶體所有權：API 不暴露裸指標，內部提供池化與可選非受控擁有層。
2.5 型別策略：float/double 為核心；複數路徑以 System.Numerics.Complex 全面覆蓋。
2.6 Deliverables: `Architecture.md`, `DataLayoutSpec.md`, `MemoryOwnershipSpec.md`。
2.7 DoD: 架構決策有 ADR，並完成跨型別端到端原型路徑。

#### 3. W2 核心運算平面
3.1 A 軌（張量運算）: 
Level 0 記憶體與拓樸 (Memory & Topology): 
形狀與跨步管理 (Shape & Strides)：座標到記憶體偏移量 $O(1)$ 映射。
廣播機制引擎 (Broadcasting Engine)：無記憶體配置的維度虛擬擴展。
視圖與切片 (Views & Slicing)：基於 Span<T> 的步進切片。
Padding 策略 (Zero, Edge, Periodic)。
Stride tricks。
Level 1 泛用函數與逐元素運算 (Universal Functions - UFuncs):
純量算術 (Arithmetic)：加減乘除、冪次。連續記憶體下啟動 SIMD。
超越數學函數 (Transcendental)：$\exp, \log, \sin, \cos$。
邏輯與遮罩 (Logical & Masking)：布林比較與遮罩張量生成。
複數專用 (實/虛部擷取、共軛)。
Level 2 結構操作與幾何變換 (Structural Manipulations):
形狀重塑與攤平 (Reshape & Flatten & Transpose)、擴展與壓縮 (ExpandDims & Squeeze)。
轉置與軸對換 (Transpose & SwapAxes)：支援 N 維任意軸排列。
拼接與堆疊 (Concatenate & Stack)。
排序與搜尋 (argsort, lexsort, searchsorted, where, nonzero)。
Level 3 降維與聚合 (Reductions & Aggregations):
數值聚合 (Sum, Mean, Var, Max/Min)：實作 Kahan 補償求和降低誤差。
索引聚合 (ArgMax / ArgMin)。
累加運算 (CumSum, CumProd)。
統計量 (Quantile, Median, NaN-safe reductions)。
Level 4 高階張量代數 (Advanced Tensor Algebra):
張量收縮 (Tensor Contraction)：降維轉置後對接 Gemm。
愛因斯坦求和約定 (Einstein Summation - Einsum)：依字串簽名自動解析高階收縮。
克羅內克積 (Kronecker Product)。
Level 4 頻域與信號 (Signal & Frequency domain):
1D/N-D FFT。
實數 rfft。
IFFT。
離散捲積 (Convolution)。
3.2 B 軌（線性代數）:
BLAS 風格核心: Axpy, Norms, Gemv, Gemm (支援 Cache-oblivious)。
空間轉換與幾何: 正交投影 (Orthogonal projection)、Gram-Schmidt 正交化、距離度量矩陣 (Euclidean, Cosine, Mahalanobis)。
基本運算: 基本四則運算、反矩陣、行列式、特徵值、特徵向量、轉置、共軛轉置、內積、外積、點積、Norms、矩陣向量乘法、矩陣矩陣乘法。
應用求解器: 稠密線性求解 (Dense Linear System Solvers)、廣義反矩陣 (Pseudoinverse)、Eigen Solvers、軌跡(Trace)/秩(Rank) 穩定估算、稀疏矩陣算子基礎 (CSR/CSC 矩陣向量乘法)。
矩陣分解: LU、QR、Cholesky、SVD、Schur。 分解、線性方程、最小平方、條件數、殘差。
3.3 C 軌（ODE）:
雅可比矩陣估算器、Euler、改良 Euler、RK4、Dormand-Prince (RK45 - 自適應)、BDF (Backward Differentiation Formula)、Radau 法、剛性入口、自適應步長控制、容差錯誤估算、密集輸出內插 (Dense output)。
3.4 D 軌（共用數值）:
尋根 (Root Finding): Newton-Raphson、割線法 (Secant)、Brent 法。
微積分: 梯形/辛普森法則、高斯求積 (Gaussian Quadrature)、有限差分 (Finite differences)。
最佳化 (Optimization): 梯度下降 (Gradient Descent)、L-BFGS、拉格朗日乘數基礎。
插值: 多項式插值、Cubic Splines (三次方樣條)、基於徑向基函數 (RBF)。
隨機與統計 : RNG 狀態管理、Ziggurat 快速分佈生成 (Normal, Uniform, U(0,1))、協方差矩陣 (Covariance)、皮爾遜相關係數。
3.5 Deliverables: `TensorCore/*`, `LinalgCore/*`, `OdeCore/*`, `NumericalCore/*`。
3.6 DoD: 每個演算法有正確性測試、邊界測試與基準測試骨架。同功能採用最高效能演算法 (Rule 21)，並通過基準正確性與效能閘門。

#### 4. W3 NumPy 風格 API 到 C# 映射
4.1 映射 shape/stride、broadcast、reduction、axis-aware 操作。
4.2 檢閱所有完成模組並提供對應的模組命名空間。
4.3 以 C# 語義取代 Python 動態機制（明確型別、例外模型、不可變視圖）。
4.4 分離高階 API 與低階 kernel API，確保可維護性與性能可控。
4.5 Deliverables: `ApiDesign.md`, `ApiSurface.cs`, `UsageSamples.md`。
4.6 DoD: API 審查通過，並有最小可用範例覆蓋主要操作路徑。

#### 5. W4 性能工程與 unsafe 啟用閘門
5.1 建立 BenchmarkDotNet 基準：尺寸、型別、執行緒、記憶體配置統計。
5.2 unsafe 準入規則：必須有瓶頸證據與回歸保護，否則維持 Span-first。
5.3 SIMD/intrinsics 導入：先 contiguous hot loop，後對齊與 fallback。
5.4 平行策略：上限為 Environment.ProcessorCount，依 compute-bound/memory-bound/small-size 調整。
5.4.1 TensorCore：逐元素運算優先 managed SIMD，對 reduction/convolution 預設保守不開平行，避免競態與過度配置。
5.4.2 LinalgCore：Gemv/Gemm 採 row-level gated parallel；Axpy/Dot/Gemv 內核採 managed SIMD + scalar fallback。
5.4.3 OdeCore：JacobianEstimator 提供 opt-in parallel（僅在 system 可重入且維度夠大時啟用）。
5.4.4 NumericalCore：Covariance/Correlation 採 managed SIMD；大型資料集提供 opt-in parallel 聚合路徑。
5.5 Deliverables: `Benchmarks/*`, `PerfGate.md`, `UnsafeRationale.md`。
5.6 DoD: 目標 hot path 達標，且無顯著配置回歸。

#### 6. W5 正確性、穩定性、文件與發布
6.1 測試分層：單元、性質、已知解回歸、複數路徑、極端條件。
6.2 數值驗證：收斂階、誤差界、病態矩陣、剛性 ODE。
6.3 文件與發布：使用說明、API 參考、演算法選型、性能報告、語意化版本策略。
6.4 Deliverables: `tests/*`, `ValidationReport.md`, `ReleasePlan.md`。
6.5 DoD: 「追溯矩陣 -> 測試 -> 基準」三向連結完整。

## 依賴關係與平行化
1. W0 完成後，W1 才可定稿。
2. W2 與 W3 可在 W1 後平行推進。
3. W4 依賴 W2/W3 的可執行實作。
4. W5 在 W2/W3/W4 穩定後收斂。

## 32 條規則落地控制點
1. 公開介面安全可維護: API 僅暴露 Span/ReadOnlySpan 與受控抽象。
2. hot path 可 unsafe + SIMD + 多執行緒: 僅限性能閘門通過後的 kernel。
3. 降低配置與 GC 壓力: 使用指標，優先 stackalloc、ArrayPool、工作區重用。
4. 支援張量 + 線代 + ODE 共用資料結構: 統一 shape/stride/workspace 規範。
5. API 邊界用 Span: 不暴露裸指標至公共 API。
6. 大部分流程先用 Span: 預設實作策略為 Span-first。
7. 少數瓶頸再 unsafe: 每項 unsafe 需附 benchmark 與回歸測試。
8. 非受控記憶體擁有層 unsafe: 集中於單一 ownership layer。
9. PInvoke/intrinsics/手動對齊 hot loop 可 unsafe: 僅限已驗證路徑。
10. 無 benchmark 不全面 unsafe: 納入 PR gate。
11. 同功能採高效演算法版本: 演算法選型需附比較依據。
12. 執行緒以上限與工作型態調整: 以 ProcessorCount 為上限並動態調參。
13. 支援複數系統: 主要線代與 ODE 路徑提供 Complex 支援與測試。
14. API 設計需考慮使用者習慣與語言特性: 以 NumPy 為參考但適配 C# 語義。
15. 先規劃全量範圍，不拆 M1: 初版計畫涵蓋完整功能清單，後續依章節目錄增量修訂。
16. 先產生初版計畫，後續可依 PDF 目錄做增量修訂: 初版以功能分類建立追溯框架，待章節目錄後精準對位。
17. Algebra/Topology 僅作缺漏補強，不獨立為拓樸模組: 以 Numerical methods 為核心，Algebra/Topology 僅補齊數值計算相關缺項，不獨立成模組。
18. 平台鎖定 .NET 8 (LTS) + x64 Windows/Linux: 以 .NET 8 為基礎，優先支持 x64 Windows 與 Linux 平台。
19. 每個模組都需要附加詳細說明文件與註解，包含使用範例與性能建議: 每個核心模組需提供詳細的 API 參考、使用範例與性能優化建議，以提升可用性與採用率。
20. 風險登錄與緩解措施需持續更新，並在每個里程碑審查: 風險登錄表需定期更新，並在每個里程碑審查時評估風險狀態與緩解措施的有效性。
21. 功能的實作時需要依照功能模組需求尋找同功能最高效能演算法來實現。
22. 針對每個功能模組，需定義清晰的 API 設計原則與實作指南，以確保一致性與可維護性: 每個模組需有明確的 API 設計原則與實作指南，涵蓋命名規範、參數設計、錯誤處理等方面，以確保整體庫的一致性與可維護性。
23. 在性能優化過程中，需保持對正確性的嚴格驗證，避免因優化而引入數值不穩定或錯誤: 性能優化需伴隨嚴格的正確性驗證，包括收斂性測試、誤差分析等，以確保優化不會犧牲數值穩定性或引入錯誤。
24. 針對不同的使用場景，需提供適當的 API 層次與抽象，以滿足不同用戶的需求: 提供多層次的 API 抽象，從簡單易用的高階接口到靈活可控的低階接口，以滿足不同用戶的需求與使用場景。
25. 需定期進行代碼審查與性能分析，以確保代碼質量與性能目標的達成: 定期進行代碼審查與性能分析，確保代碼質量符合標準，並持續監控性能指標以達成預設目標。
26. 需建立完善的測試覆蓋，包括單元測試、集成測試與性能測試，以確保庫的穩定性與可靠性: 建立全面的測試體系，涵蓋單元測試、集成測試與性能測試，確保庫的穩定性與可靠性。
27. 需考慮未來的擴展性與兼容性，在設計 API 與架構時預留擴展點，以便未來添加新功能或支持新平台: 在 API 與架構設計中預留擴展點，考慮未來可能的功能擴展與平台支持，確保庫的長期可維護性與兼容性。
28. 需建立清晰的版本控制與發布流程，確保每次發布都伴隨完整的變更記錄與升級指南: 建立嚴謹的版本控制與發布流程，每次發布需提供完整的變更記錄與升級指南，以便用戶順利過渡到新版本。
29. 維度優化視圖： 系統支援 N 維，但針對 1D 向量 與 2D 矩陣 提供專屬高效能視圖（如 AsSpan1D(), AsSpan2D()）。大於 3 維的張量將透過跨步 (Strided) 邏輯退化至 1D/2D 核心處理。
30. 以性能門檻為基準的 unsafe 啟用： 針對 hot path 的核心運算，設定明確的性能門檻（如吞吐量、延遲、配置），只有在達到這些門檻且有明確瓶頸證據的情況下，才允許引入 unsafe 實作。
31. 以數據驅動的演算法選型： 每個功能的演算法選型需基於數據驅動的分析，提供性能比較與適用場景說明，確保選擇最合適的演算法版本。
32. 以使用者需求為導向的 API 設計： API 設計需充分考慮使用者需求與使用習慣，提供直觀易用的接口，同時兼顧靈活性與性能。

## 里程碑與驗收閘門
1. Gate A（W0+W1）: 追溯矩陣、規則清單、架構與資料規格凍結。
2. Gate B（W2+W3）: 核心功能可運行，API 路徑可用。
3. Gate C（W4）: 主要 hot path 性能達標，unsafe 使用有證據。
4. Gate D（W5）: 測試與驗證報告完成，可發布候選版。

## 風險登錄（v1）
1. 風險: PDF 內容未解析導致需求偏差。
Mitigation: 以章節目錄補全追溯矩陣，逐輪差異校正。
2. 風險: 過早 unsafe 化造成可維護性下降。
Mitigation: 以 PerfGate 強制 benchmark 先決條件。
3. 風險: ODE 與線代資料模型分裂。
Mitigation: 在 W1 凍結共用 layout/stride/workspace 規格。
4. 風險: 複數路徑被延後造成架構返工。
Mitigation: 從 W1 即納入 Complex 測試案例與 API 簽章。

## 立即執行項目
1. 建立 `TraceabilityMatrix.md` 初稿模板（功能 ID、來源章節、實作節點、測試節點、性能節點）。
2. 建立 `RuleComplianceChecklist.md`（32 條規則逐條驗證欄位）。
3. 建立 `PerfGate.md`（unsafe 準入標準與量測指標）。
4. 建立 `ApiDesign.md`（NumPy 概念到 C# API 的對照表）。

**Relevant files**
- d:/Project/LAL/開發計畫與規格書.md — 主規格來源，需做功能追溯基線
- d:/Project/LAL/Numerical methods_algorithms and tools in C#.pdf — 缺項補強來源1
- d:/Project/LAL/Algebra_Topology_Dierential Calculus and Optimization Theory For Computer Science and Machine Learning.pdf — 缺項補強來源2
- d:/Project/LAL/numpy-user.pdf — API 風格與使用層行為映射參考
- d:/Project/LAL/numpy-ref.pdf — 模組分層與操作語義映射參考

**Verification**
1. 規格覆蓋率：功能追溯矩陣中「Numerical methods 功能項」覆蓋率達 100%，每項對應至少 1 個測試與 1 個實作節點。
2. 規則符合度：32 條規則逐條打勾，unsafe 項目必須附 benchmark 與安全邊界說明。
3. 正確性：線代與 ODE 的標準問題集通過率 100%，並保留誤差閾值報告。
4. 性能：hot path 在目標尺寸下達成預設門檻（吞吐/延遲/配置），未達標不得擴大 unsafe 範圍。
5. 可維護性：公開 API 僅暴露安全抽象，禁止洩漏內部非受控細節。

**Decisions**
- 已確認：先產生初版計畫，後續可依參考文件檔目錄做增量修訂。
- 已確認：平台鎖定 .NET 8 (LTS) + x64 Windows/Linux。
- 已確認：採一次規劃全量範圍，不拆 M1。
- 假設：目前 PDF 內容未直接可讀，功能清單先以標準數值方法分類建立追溯框架，搜尋對應目錄後精準對位並上網搜尋資料補齊。