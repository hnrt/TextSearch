package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.service.FileGroupService;

@SpringBootTest
public class FileGroupControllerTests {

	private MockMvc mockMvc;

	@Mock
	private FileGroupService fileGroupService;

	@InjectMocks
	private FileGroupController fileGroupController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(fileGroupController).build();
    }

	@Test
	public void getGroups_successful() throws Exception {
		when(fileGroupService.getGroups()).thenReturn(new FileGroupInfo[] {
				new FileGroupInfo(1, "foo", "root"),
				new FileGroupInfo(2, "bar", "root"),
				new FileGroupInfo(3, "baz", "root")
		});
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/groups"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$[0].name").value("foo"))
        	.andExpect(jsonPath("$[1].name").value("bar"))
        	.andExpect(jsonPath("$[2].name").value("baz"));
	}

}
