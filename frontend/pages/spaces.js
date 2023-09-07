import {useState} from "react";
import Head from "next/head";
import MatrixSpaces from "../components/matrixSpaces";

export default function Spaces() {
  return <>
    <Head>
      <title>Matrix Spaces - DiscoFriends</title>
    </Head>
    <MatrixSpaces />
  </>
}