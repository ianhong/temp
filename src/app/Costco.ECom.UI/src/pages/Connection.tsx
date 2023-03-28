import PageTitle from "../components/PageTitle"
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { solid } from '@fortawesome/fontawesome-svg-core/import.macro'
import { useState } from "react"
import inventoryService from "../services/inventory.service"
import { Connection as ConnectionModel } from "../models/connection"
import Modal from '../components/Modal'

const Connection = () => {

    const [loading, setLoading] = useState(false)
    const [showModal, setShowModal] = useState(false)
    const [messageModal, setMessageModal] = useState('')
    const [url, setUrl] = useState('')
    const [serverResponse, setServerResponse] = useState(null)

    const tryParseJSON = (content: any) => {
        try {
            return JSON.parse(content)
        } catch {
            return content
        }
    }
    const handleSubmit = async (e: any) => {
        try {
            e.preventDefault()

            if (!url) {
                setMessageModal('Please provide an API URL')
                setShowModal(true)

                setTimeout(() => setShowModal(false), 1500)
                return
            }

            setLoading(true)
            setServerResponse(null)

            const dto: ConnectionModel = {
                ConnectionUrl: url
            }
            const response = await inventoryService.checkConnection(dto)

            if (response.content) {
                response.content = tryParseJSON(response.content)
            }

            setServerResponse(response)

        } catch (error: any) {
            console.log('Error Response', error)
            setServerResponse(error)
        } finally {
            setLoading(false)
        }
    }
    const handleReset = () => {
        setUrl('')
        setServerResponse(null)
    }
    const handleChange = (e: any) => setUrl(e.target.value)

    return (
        <>
            <PageTitle title='Connection'>
                <FontAwesomeIcon icon={solid('heart-pulse')} className='has-text-primary pr-2' />
            </PageTitle>

            <Modal title={'Message'} message={messageModal} showModal={showModal} setShowModal={setShowModal} />

            <form>
                <div className='columns'>
                    <div className="column">
                        <input
                            required
                            className="input"
                            type="url"
                            value={url}
                            disabled={loading}
                            placeholder='Enter API URL (e.g http://jsonplaceholder.typicode.com/users)'
                            onChange={handleChange} />
                    </div>
                    <div className="column is-4 is-3-desktop">
                        <div className="is-flex">
                            <button type="submit" className={`button is-primary mr-2 is-flex-grow-1 ${loading ? 'is-loading' : ''}`} onClick={handleSubmit}>
                                <FontAwesomeIcon icon={solid('bolt')} className='pr-2' />
                                Fetch
                            </button>
                            <button className='button is-secondary is-flex-grow-1' type="reset" disabled={loading} onClick={handleReset}>
                                <FontAwesomeIcon icon={solid('arrows-rotate')} className='pr-2' />
                                Reset
                            </button>
                        </div>
                    </div>
                </div>
            </form>

            {
                serverResponse && (
                    <pre className="is-code is-smooth mt-5">
                        <code>
                            {JSON.stringify(serverResponse, null, 2)}
                        </code>
                    </pre>
                )
            }

        </>
    )
}

export default Connection