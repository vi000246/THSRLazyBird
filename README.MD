需求:
翻頁的邏輯，能否一次載入全部車次
schedule job
dockerfile
自訂參數，能否用GUI輸入
參數(訂票代號，ID，執行interval，是否啟動)
訂票成功通知、執行失敗通知
能否多線程執行
login timeout ，retry的機制
能否multiple login
能直接用line 掃qrcode就接到line notify嗎
需要DB嗎(for schedule job，儲存參數)

URL分析:

登入頁面:
https://irs.thsrc.com.tw/IMINT/?wicket:bookmarkablePage=:tw.com.mitac.webapp.thsr.viewer.History

輸入資料按submit的request url:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:1:HistoryForm::IFormSubmitListener
應該會在這裡取得cookie

這是訂票頁面:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:8::

按下變更行程request 的url:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:14:HistoryDetailsForm::IFormSubmitListener

變更行程頁面:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:15::

變更行程表單submit:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:15:HistoryDetailsModifyTripS1Form::IFormSubmitListener

搜尋車次頁面:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:20::

更早車次:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:20:HistoryDetailsModifyTripS2Form:TrainQueryDataViewPanel2:PreAndLaterTrainContainer:preTrainLink::IBehaviorListener&wicket:behaviorId=0&random=0.29615310315552357

更晚車次:
https://irs.thsrc.com.tw/IMINT/?wicket:interface=:20:HistoryDetailsModifyTripS2Form:TrainQueryDataViewPanel2:PreAndLaterTrainContainer:laterTrainLink::IBehaviorListener&wicket:behaviorId=0&random=0.767041917292927

訂票頁面cookie分析:
