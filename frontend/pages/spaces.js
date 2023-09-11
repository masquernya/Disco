import {useState} from "react";
import Head from "next/head";
import MatrixSpaces from "../components/matrixSpaces";
import api from "../lib/api";

export default function Spaces(props) {
  return <>
    <Head>
      <title>Matrix Spaces - DiscoFriends</title>
    </Head>
    <MatrixSpaces spaces={props.spaces} />
  </>
}

export async function getStaticProps() {
  const spaces = await api.request('/api/matrixspace/AllSpaces');
  return {
    revalidate: 60,
    props: {
      spaces: spaces.body,
    }
  };
}