package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.service.FileGroupService;

@RestController
public class FileGroupController {

	@Autowired
	private FileGroupService service;

	@RequestMapping(value="/groups",method=RequestMethod.GET)
	public ValuesResponse getGroups() {
		return service.getGroups();
	}

}
