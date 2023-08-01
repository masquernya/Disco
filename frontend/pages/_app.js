import 'bootstrap/dist/css/bootstrap.css';
import Navbar from "../components/navbar";
import Footer from "../components/footer";
import {useState} from "react";
import {setUserFunctions} from "../lib/globalState";
import Script from "next/script";

export default function MyApp({ Component, pageProps }) {
  const [user, setUser] = useState(null);
  setUserFunctions(user, setUser);

  return <>
    <Script async={true} src="https://analytics.discofriends.net/script.js" data-website-id="9dd22158-2172-4cd1-aafd-e1b67dc50ca7" />
    <Navbar />
    <Component {...pageProps} />
    <Footer />
  </>
}
  