import MeComponent from '../components/me';
import Head from "next/head";
export default function Me(props) {
  return<>
    <Head>
      <title>My Settings - DiscoFriends</title>
    </Head>
    <MeComponent />
    </>
}