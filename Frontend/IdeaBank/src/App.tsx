import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

const API_URL = import.meta.env.DEV ? '/api/Ideas' : 'https://localhost:7036/api/Ideas'
const STORAGE_KEY = 'bank-of-ideas'
const ideaTypes = ['Security', 'Identification', 'Technology', 'Efficiency', 'Entertainment', 'Evolution']
const statuses = ['Proposed', 'InReview', 'Approved', 'Rejected']

type Status = (typeof statuses)[number]

interface Idea {
  id: string
  title: string
  description: string
  status: Status
  createdAt: string
  updatedAt: string
  tags: string[]
}

interface IdeaForm {
  title: string
  description: string
  tags: string[]
}

const emptyForm: IdeaForm = { title: '', description: '', tags: [] }

function readStoredIdeas(): Idea[] {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    return saved ? JSON.parse(saved) : []
  } catch {
    return []
  }
}

function saveIdeas(ideas: Idea[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(ideas))
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', year: 'numeric', hour: 'numeric', minute: '2-digit' }).format(new Date(value))
}

function App() {
  const [ideas, setIdeas] = useState<Idea[]>(readStoredIdeas)
  const [form, setForm] = useState<IdeaForm>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [draft, setDraft] = useState<Pick<Idea, 'title' | 'description' | 'status'>>({ title: '', description: '', status: 'Proposed' })
  const [filter, setFilter] = useState<'All' | Status>('All')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const loadIdeas = async () => {
      try {
        const response = await fetch(API_URL)
        if (!response.ok) throw new Error('Unable to load ideas')
        const remoteIdeas = await response.json() as Idea[]
        saveIdeas(remoteIdeas)
        setIdeas(remoteIdeas)
      } catch {
        setNotice('Showing saved ideas. The API could not be reached.')
      } finally {
        setLoading(false)
      }
    }
    void loadIdeas()
  }, [])

  const visibleIdeas = useMemo(() => filter === 'All' ? ideas : ideas.filter((idea) => idea.status === filter), [filter, ideas])

  const updateIdeas = (nextIdeas: Idea[]) => {
    setIdeas(nextIdeas)
    saveIdeas(nextIdeas)
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setNotice('')
    if (!form.title.trim() || !form.description.trim()) {
      setError('Add a title and description before submitting.')
      return
    }
    if (!form.tags.length) {
      setError('Select at least one idea type to continue.')
      return
    }

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: form.title.trim(), description: form.description.trim(), status: 'Proposed', tags: form.tags }),
      })
      if (!response.ok) throw new Error('Unable to save idea')
      const created = await response.json() as Idea
      updateIdeas([created, ...ideas])
      setForm(emptyForm)
      setNotice('Idea registered successfully.')
    } catch {
      setError('The idea could not be saved. Check the API connection and try again.')
    }
  }

  const startEditing = (idea: Idea) => {
    setEditingId(idea.id)
    setDraft({ title: idea.title, description: idea.description, status: idea.status })
    setError('')
  }

  const updateIdea = async (idea: Idea) => {
    setError('')
    try {
      const response = await fetch(`${API_URL}/${idea.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: idea.id, ...draft, tags: idea.tags, createdAt: idea.createdAt, updatedAt: idea.updatedAt }),
      })
      if (!response.ok) throw new Error('Unable to update idea')
      const updated = await response.json() as Idea
      updateIdeas(ideas.map((item) => item.id === idea.id ? updated : item))
      setEditingId(null)
      setNotice('Idea updated successfully.')
    } catch {
      setError('The update could not be saved. Check the API connection and try again.')
    }
  }

  const deleteIdea = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this idea? This action cannot be undone.')) return

    setError('')
    try {
      const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' })
      if (!response.ok) throw new Error('Unable to delete idea')
      updateIdeas(ideas.filter((idea) => idea.id !== id))
      setNotice('Idea deleted.')
    } catch {
      setError('The idea could not be deleted. Check the API connection and try again.')
    }
  }

  return (
    <main className="app-shell">
      <header className="topbar"><div className="brand-mark">BI</div><span>Idea workspace</span><span className="api-status"><i /> API connected</span></header>
      <section className="intro"><p className="eyebrow">COLLECTIVE THINKING</p><h1>Idea Tracker</h1><p>Turn bright sparks into meaningful progress.</p></section>

      <section className="content-grid">
        <form className="idea-form panel" onSubmit={handleSubmit}>
          <div className="panel-heading"><div><p className="eyebrow">NEW SUBMISSION</p><h2>Register an idea</h2></div><span className="step-badge">01</span></div>
          <label>Title<input value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} placeholder="Give your idea a clear name" /></label>
          <label>Description<textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} placeholder="What problem does this solve?" rows={5} /></label>
          <label>Status<select value="Proposed" disabled><option>Proposed</option></select></label>
          <fieldset><legend>Idea types <span>Select all that apply</span></legend><div className="tag-grid">{ideaTypes.map((tag) => <label className={`tag-option ${form.tags.includes(tag) ? 'selected' : ''}`} key={tag}><input type="checkbox" checked={form.tags.includes(tag)} onChange={() => setForm({ ...form, tags: form.tags.includes(tag) ? form.tags.filter((item) => item !== tag) : [...form.tags, tag] })} /><span>{tag}</span></label>)}</div></fieldset>
          {error && <p className="message error">{error}</p>}{notice && <p className="message success">{notice}</p>}
          <button className="primary-button" type="submit">Submit idea <span>-&gt;</span></button>
        </form>

        <section className="ideas-section"><div className="section-heading"><div><p className="eyebrow">YOUR PIPELINE</p><h2>Idea register <span>{ideas.length}</span></h2></div><label className="filter-control">Filter by status<select value={filter} onChange={(event) => setFilter(event.target.value as 'All' | Status)}><option>All</option>{statuses.map((status) => <option key={status}>{status}</option>)}</select></label></div>
          <div className="table-wrap"><table><thead><tr><th>Idea</th><th>Description</th><th>Status</th><th>Created</th><th>Updated</th><th>Tags</th><th aria-label="Actions" /></tr></thead><tbody>{loading ? <tr><td className="empty-state" colSpan={7}>Loading ideas...</td></tr> : visibleIdeas.length === 0 ? <tr><td className="empty-state" colSpan={7}>No ideas match this filter yet.</td></tr> : visibleIdeas.map((idea) => <tr key={idea.id}>{editingId === idea.id ? <><td><input className="table-input" value={draft.title} onChange={(event) => setDraft({ ...draft, title: event.target.value })} /></td><td><textarea className="table-input" value={draft.description} onChange={(event) => setDraft({ ...draft, description: event.target.value })} /></td><td><select className="table-input" value={draft.status} onChange={(event) => setDraft({ ...draft, status: event.target.value as Status })}>{statuses.map((status) => <option key={status}>{status}</option>)}</select></td></> : <><td className="idea-title">{idea.title}</td><td>{idea.description}</td><td><span className={`status status-${idea.status.toLowerCase()}`}>{idea.status}</span></td></>}<td>{formatDate(idea.createdAt)}</td><td>{formatDate(idea.updatedAt)}</td><td><div className="table-tags">{idea.tags.map((tag) => <span key={tag}>{tag}</span>)}</div></td><td className="actions">{editingId === idea.id ? <button title="Save update" onClick={() => void updateIdea(idea)}>Save</button> : <button title="Edit idea" onClick={() => startEditing(idea)}>Edit</button>}<button className="delete-button" title="Delete idea" onClick={() => void deleteIdea(idea.id)}>Delete</button></td></tr>)}</tbody></table></div>
        </section>
      </section>
      <footer><span>Bank of Ideas</span><span>Ideas are stored locally and synced with your API.</span></footer>
    </main>
  )
}

export default App
