import React, { useEffect, useMemo, useState } from "react";
import debounce from "lodash.debounce";
import { toast } from "react-hot-toast";
import api from "../api";

export default function ModulesTopicsManagement() {
  const [topics, setTopics] = useState([]);
  const [topicSearch, setTopicSearch] = useState("");
  const [filteredTopics, setFilteredTopics] = useState([]);
  const [selectedTopicId, setSelectedTopicId] = useState("");
  const [loading, setLoading] = useState(false);
  const [totalTopics, setTotalTopics] = useState(0);
  const pageSize = 10; 


  const [topicForm, setTopicForm] = useState({ topicName: "", description: "" });


  const [moduleForm, setModuleForm] = useState({
    moduleName: "",
    description: "",
    contentLink: "",
    orderNo: 0,
  });

 
  const [modules, setModules] = useState([]); 
  const [myModules, setMyModules] = useState([]);


  const [topicPage, setTopicPage] = useState(1);


  const [actionsLog, setActionsLog] = useState([]);


  const searchTopics = async (q, page = 1) => {
    setLoading(true);
    try {
      const res = await api.get("/VLearnTopic/search", { params: { q: q || "", page, size: pageSize } });
      const payload = res.data || {};
      const items = payload.items ?? payload.items ?? [];
      setFilteredTopics(Array.isArray(items) ? items : []);
      setTotalTopics(Number.isFinite(payload.total) ? payload.total : payload.total ?? 0);
      setTopicPage(page);
    } catch (err) {
      console.error("searchTopics:", err);
      toast.error("Search failed");
    } finally {
      setLoading(false);
    }
  };

  const doTopicSearch = useMemo(
    () =>
      debounce((q) => {
    
        searchTopics(q, 1);
      }, 300),
    []
  );

  useEffect(() => {
    doTopicSearch(topicSearch);

    return () => doTopicSearch.cancel();
  }, [topicSearch, doTopicSearch]);

  useEffect(() => {
    searchTopics("", 1);

  }, []);


  const onTopicPageChange = (p) => {
    if (p < 1) p = 1;
    searchTopics(topicSearch, p);
  };

  const reloadTopics = () => searchTopics(topicSearch, topicPage);

  const handleCreateTopic = async (e) => {
    e?.preventDefault?.();
    if (!topicForm.topicName?.trim()) return toast.error("Topic name is required");
    try {
      const payload = { topicName: topicForm.topicName.trim(), description: topicForm.description || null };
      const res = await api.post("/VLearnTopic", payload);
      toast.success("Topic created");
      addActionLog(`Created topic "${payload.topicName}"`);
      setTopicForm({ topicName: "", description: "" });

      await searchTopics("", 1);
      const createdId = res.data?.topicId ?? res.data?.TopicId;
      if (createdId) setSelectedTopicId(createdId);
    } catch (err) {
      console.error(err);
      const msg = err?.response?.data ?? err?.message ?? "Create topic failed";
      toast.error(String(msg));
    }
  };


  const loadModulesForTopic = async (topicId) => {
    if (!topicId) {
      setModules([]);
      return;
    }
    setLoading(true);
    try {
      const res = await api.get(`/VLearnModule/topic/${topicId}/modules`);
      setModules(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load modules for topic");
    } finally {
      setLoading(false);
    }
  };


  const loadMyModulesForTopic = async (topicId) => {
    if (!topicId) {
      setMyModules([]);
      return;
    }

    setLoading(true);
    try {

      const res = await api.get(`/VLearnModule/topic/${topicId}/modules`);
 
      setMyModules(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error("loadMyModulesForTopic:", err);
   
      if (err?.response?.status === 401) {
        toast.error("Please login to see your modules.");
      } else {
        toast.error("Failed to load modules for the selected topic.");
      }
      setMyModules([]);
    } finally {
      setLoading(false);
    }
  };


  useEffect(() => {
    if (!selectedTopicId) {
      setModules([]);
      setMyModules([]);
      return;
    }
    loadModulesForTopic(selectedTopicId);
    loadMyModulesForTopic(selectedTopicId);
  }, [selectedTopicId]);


  const handleCreateModule = async (e) => {
    e?.preventDefault?.();
    if (!selectedTopicId) return toast.error("Select a topic first");
    if (!moduleForm.moduleName?.trim()) return toast.error("Module name is required");
    try {
      const payload = {
        moduleName: moduleForm.moduleName.trim(),
        description: moduleForm.description || null,
        contentLink: moduleForm.contentLink || null,
        orderNo: Number.isNaN(Number(moduleForm.orderNo)) ? 0 : Number(moduleForm.orderNo),
      };
      const res = await api.post(`/VLearnModule/topic/${selectedTopicId}/modules`, payload);
      toast.success("Module created");
      addActionLog(`Module "${payload.moduleName}" created under topic`);
      await loadModulesForTopic(selectedTopicId);
      await loadMyModulesForTopic(selectedTopicId);
      setModuleForm({ moduleName: "", description: "", contentLink: "", orderNo: 0 });
    } catch (err) {
      console.error(err);
      const msg = err?.response?.data ?? err?.message ?? "Create module failed";
      toast.error(String(msg));
    }
  };

  const addActionLog = (message) =>
    setActionsLog((prev) => [{ id: Date.now(), message, ts: new Date().toLocaleString() }, ...prev].slice(0, 20));
  const getTopicId = (t) => t.topicId ?? t.TopicId;
  const getTopicName = (t) => t.topicName ?? t.TopicName ?? t.Topic_Name ?? "";

  const getModuleId = (m) => m.moduleId ?? m.ModuleId;
  const getModuleName = (m) => m.moduleName ?? m.ModuleName ?? "";
  const getModuleOrder = (m) => m.orderNo ?? m.OrderNo ?? 0;

  const totalTopicPages = Math.max(1, Math.ceil((totalTopics || 0) / pageSize));
  const paginatedTopics = filteredTopics; 

  return (
    <div className="bg-white p-8 rounded-xl shadow-md border border-gray-200 space-y-6">
      <h2 className="text-2xl font-bold text-gray-800">Topics & Modules</h2>

      
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm flex items-center justify-between">
        <div>
          <strong>Manage topics & modules</strong>
          <div className="text-sm text-gray-600">Create categories (topics) and modules, view public modules and your progress.</div>
        </div>
        <div className="text-sm text-gray-600">Status: {loading ? "Loading..." : "Ready"}</div>
      </div>

      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-3">Create Topic (Category)</h3>
        <form onSubmit={handleCreateTopic} className="flex flex-wrap gap-3 items-center">
          <input
            placeholder="Topic name"
            value={topicForm.topicName}
            onChange={(e) => setTopicForm({ ...topicForm, topicName: e.target.value })}
            className="border p-2 rounded-md w-72"
          />
          <input
            placeholder="Description (optional)"
            value={topicForm.description}
            onChange={(e) => setTopicForm({ ...topicForm, description: e.target.value })}
            className="border p-2 rounded-md w-96"
          />
          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded-md">Create Topic</button>
          <button type="button" onClick={() => setTopicForm({ topicName: "", description: "" })} className="px-3 py-2 rounded-md border">
            Reset
          </button>
        </form>
      </div>


      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-lg font-semibold">Topics</h3>
          <div className="flex gap-2">
            <input
              placeholder="Search topics..."
              value={topicSearch}
              onChange={(e) => setTopicSearch(e.target.value)}
              className="border p-2 rounded-md"
            />
            <button onClick={() => { setTopicSearch(""); searchTopics("", 1); }} className="px-3 py-1 border rounded">Reset</button>
            <button onClick={() => reloadTopics()} className="px-3 py-1 border rounded">Reload</button>
          </div>
        </div>

        {paginatedTopics.length === 0 ? (
          <p className="text-sm text-gray-500">No topics found.</p>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            {paginatedTopics.map((t) => {
              const id = getTopicId(t);
              return (
                <div key={id} className={`p-3 border rounded-md ${id === selectedTopicId ? "bg-blue-50 border-blue-300" : "bg-white"}`}>
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="font-medium">{getTopicName(t)}</div>
                      <div className="text-sm text-gray-600">{t.description ?? t.Description ?? ""}</div>
                    </div>
                    <div className="flex gap-2">
                      <button onClick={() => { setSelectedTopicId(id); }} className="px-3 py-1 bg-blue-200 rounded">Select</button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}

      
        <div className="mt-3 flex gap-2">
          {Array.from({ length: totalTopicPages }, (_, i) => i + 1).map((p) => (
            <button key={p} onClick={() => onTopicPageChange(p)} className={`px-3 py-1 rounded border ${p === topicPage ? "bg-blue-200" : "bg-gray-100"}`}>{p}</button>
          ))}
        </div>
      </div>

      
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-3">
          {selectedTopicId ? `Modules for: ${getTopicName(topics.find(t => getTopicId(t) === selectedTopicId) || {})}` : "Select a topic to view or add modules"}
        </h3>

   
        <form onSubmit={handleCreateModule} className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-4">
          <input
            value={moduleForm.moduleName}
            onChange={(e) => setModuleForm({ ...moduleForm, moduleName: e.target.value })}
            placeholder="Module name"
            className="border p-2 rounded-md col-span-1 md:col-span-1"
          />
          <input
            value={moduleForm.contentLink}
            onChange={(e) => setModuleForm({ ...moduleForm, contentLink: e.target.value })}
            placeholder="Content link"
            className="border p-2 rounded-md col-span-1 md:col-span-1"
          />
          <input
            value={moduleForm.orderNo}
            onChange={(e) => setModuleForm({ ...moduleForm, orderNo: e.target.value })}
            placeholder="Order no (0)"
            type="number"
            className="border p-2 rounded-md col-span-1 md:col-span-1"
          />
          <textarea
            value={moduleForm.description}
            onChange={(e) => setModuleForm({ ...moduleForm, description: e.target.value })}
            placeholder="Description"
            className="border p-2 rounded-md col-span-1 md:col-span-3"
          />
          <div className="col-span-1 md:col-span-3 flex gap-2">
            <button type="submit" className="bg-green-600 text-white px-4 py-2 rounded-md">Create Module</button>
            <button type="button" onClick={() => setModuleForm({ moduleName: "", description: "", contentLink: "", orderNo: 0 })} className="px-3 py-2 border rounded">Reset</button>
          </div>
        </form>

       
        <div>
          <h4 className="font-medium mb-2">Public Modules</h4>
          {modules.length === 0 ? (
            <p className="text-sm text-gray-500">No modules for this topic.</p>
          ) : (
            <div className="space-y-2">
              {modules.map((m) => (
                <div key={getModuleId(m)} className="p-3 border rounded-md bg-white flex items-center justify-between">
                  <div>
                    <div className="font-medium">{getModuleName(m)} <span className="text-xs text-gray-500">#{getModuleOrder(m)}</span></div>
                    <div className="text-sm text-gray-600">{m.description ?? m.Description ?? ""}</div>
                  </div>
                  <div className="text-sm text-gray-600"></div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

    
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-2">Recent actions</h3>
        <ul className="text-sm text-gray-600 max-h-48 overflow-y-auto space-y-1">
          {actionsLog.length === 0 ? <li>No actions yet</li> : actionsLog.map((a) => <li key={a.id}>{a.ts} — {a.message}</li>)}
        </ul>
      </div>
    </div>
  );
}
