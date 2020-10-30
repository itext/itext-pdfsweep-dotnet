#!/usr/bin/env groovy
@Library('pipeline-library')_

def repoName = "pdfSweep"
def dependencyRegex = "itextcore"
def solutionFile = "itext.cleanup.sln"
def csprojFramework = "netcoreapp2.0"

automaticDotnetBuild(repoName, dependencyRegex, solutionFile, csprojFramework)