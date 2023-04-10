import { CONSTANTS } from "../models/constants"

const Footer = () => {
    return (
        <footer className="footer">
            <div className="content has-text-centered has-text-grey">
                <p>&copy 1998 - {new Date().getFullYear()} Costco Wholesale, Inc. All rights reserved.</p>
            </div>
        </footer>
    )
}

export default Footer