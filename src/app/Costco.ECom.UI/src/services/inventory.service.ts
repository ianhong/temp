import axios from 'axios'
import { Inventory } from '../models/inventory'
import { base } from '../models/constants'
import { Connection } from '../models/connection'

const axiosInstance = axios.create({
    baseURL: `${base.inventory}/1.0/Inventory/`
})

const getInventories = async () => {
    const response = await axiosInstance.get('')
    if (response.status === 200) {
        return response.data as Inventory[]
    } else {
        throw new Error(`[${response.status}]: ${response.statusText}`)
    }
}

const getInventory = async (id: string) => {
    const response = await axiosInstance.get(`${id}`)
    if (response.status === 200) {
        return response.data as Inventory
    } else {
        throw new Error(`[${response.status}]: ${response.statusText}`)
    }
}

const checkConnection = async (connection: Connection) => {
    try {
        const response = await axiosInstance.post(`CheckConnection`, connection)
        return response.data
    } catch (error: any) {
        throw (error && error.response && error.response.data) || error
    }
}

const service = {
    getInventories,
    getInventory,
    checkConnection
}

export default service